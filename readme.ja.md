# 配列専用 LINQ

## 背景

ふと、社内のコードを見ていると、LINQ を使っている場面の8割くらいは配列を引数にとって、即座に`ToArray`して配列で返していた。
`System.Enumerable`ほどの汎用性は要らないので、その配列→配列の状況に最適化したLINQが欲しくなった。

そこで、以下のような前提で、LINQ 演算子のいくつかを最適化実装したものを作る。

- 配列専用にする
- パイプライン<sup>※1</sup>を想定しない
- 長さが、あるいは、少なくとも長さの上限<sup>※2</sup>が既知
- 長さが短い<sup>※3</sup>

(ものすごい制限が強い割には、高速化の割合は数割～(ごくごく一部)5倍程度なので、有用かどうか…)

<sup>※1</sup> 
`Select().Where.Select().OrderBy()...` みたいな多段クエリのこと。
戻り値を最初から配列にすることでオーバーヘッドを減らしているものの、
1段1段配列を作られるとむしろ大幅に遅くなる。

<sup>※2</sup>
例えば`Where`や`Distinct`の場合、結果の長さはわからないものの、
操作前の配列の長さを超えることは絶対にないので、
「長さの最大値」であれば既知になる。

<sup>※3</sup>
これは一時バッファーを`stackalloc`で確保するために必要な制限。
大きなサイズで`stackalloc`しようとすると簡単にスタックオーバーフローする。
`Span<T> buffer = len < 1024 ? (Span<T>)stackalloc T[len] : (Span<T>)new T[len]`みたいな書き方をすればこの問題は回避可能。

## 最適化手法

### 配列専用化

一般には、`foreach` ステートメントは以下のように展開される。

元:

```cs
foreach (var x in data)
{
}
```

一般の展開結果:

```cs
{
    var e = data.GetEnumerator();
    try
    {
        while (e.MoveNext())
        {
            var x = e.Current;
        }
    }
    finally
    {
        ((IDisposable)e).Dispose();
    }
}
```

それが、配列の場合は以下のように、単なる`for`ステートメントに展開される。

配列の展開結果:

```cs
for (int i = 0; i < data.Length; i++)
{
    var x = data[i];
}
```

その結果、`IEnumerable<T>`などを経由せず、配列専用に作ることで数割程度高速になる。
後述するが、`IEnumerator`の発生と、配列の範囲チェックという2つのコストが消える。

#### 制限

この最適化のためには本当に配列専用でなければならない。

(他の最適化では、「長さが既知であればいい」程度の制限なので、
`IList`や、あるいは、`IEnumerable`とセットで長さを渡すなどでも実現できる。)

かといって、「`List<T>`で渡ってきたデータを一度`ToArray`してから渡す」とかはかなり高コスト。この最適化のためには最初から配列でデータが渡ってくることが必須で、かなり用途が限られる。

#### (補足)`IEnumerator`の発生

`foreach`ステートメントの「一般の展開結果」では、
`GetEnumerator`を経由することで`IEnumerator`のインスタンスが作られる。

`List<T>`などでは、`GetEnumerator`が返す型を`List<T>`専用の構造体にすることでこのコストを回避していたりするが、これも、`IEnumerable<T>`インターフェイスを介して呼び出すとボックス化が起きてしまって、結局ヒープ確保が発生する。

ただし、1回の列挙につき1個のほんの小さなインスタンスができるだけなので、大したコストではない。

#### (補足)配列の範囲チェック

通常、配列 `a` に対してインデックスアクセスすると、
そのJITコンパイル結果には範囲チェックが挿入される。
配列の長さを越えて読み書き(バッファーオーバーラン脆弱性の原因)を起こさないようにするためなので、欠かすことはできないが、そこそこのコストになる。

ところが、以下のような`for`ステートメントを書く場合、`for`の条件式自体が範囲チェックになっているため、`x[i]`の個所に挟まるはずの範囲チェックはなくすような最適化がかかる。

```cs
for (int i = 0; i < a.Length; i++)
{
    var x = a[i];
}
```

同じような`for`ステートメントを書いても、`IList<T>`や`IReadOnlyList<T>`経由ではここまでの最適化は掛からない。

### 長さを既知にする

一般の(長さが未知の)`IEnumerable<T>`に対して`ToArray`を行おうとすると、`List<T>`が内部でやっているのと同じような「足りなくなったらバッファー用の配列を確保しなおす」というような処理が必要になる。

これに対して、長さが既知なら、最初に1個だけ、所望の長さの配列を確保して、そこにデータをコピーしていけばいいのでかなり負担が少ない。

#### 制限

長さが既知であればいいので、`IList<T>`なども使える。
それでも、一般の`IEnumerable<T>`に対して使えないのはそこそこ不便な制限になる。

### ハッシュテーブルの専用実装化

`Distinct`や`GroupBy`などの実装には、内部的にハッシュテーブルを使うことになる。

.NET でハッシュテーブルというと、`Dictionary<TKey, TValue>`や`HashSet<T>`だが、
これら標準のものを使うよりも最適化できるポイントがいくつかある。

- `Distinct`などの内部で使う場合、`Remove`は考えなくていい
  - `Remove`が不要であれば、ハッシュテーブルの実装はかなりシンプルになる
- 長さが既知であれば、バッファーの作り直しが必要ない
  - ハッシュテーブル内のバッファーの作り直しは結構負担が大きいので、これがなくなると結構速くなる

#### 制限

「`Remove`は考えなくていい」だけであれば特に制限は掛からない。
たぶん、標準ライブラリの`System.Linq.Enumerable`の`Distinct`の実装ももっと速くできると思う。

「長さが既知」の制限は前節参照。

### 一時バッファーの`stackalloc`化

前節のハッシュテーブル等は、メソッド内でしか使わない一時バッファーになる。

こういう寿命がはっきりしているものに対してmanagedヒープを使うのはもったいないので、
スタック上に一時バッファーを確保したい。
幸い、C#では、条件さえ満たせば`stackalloc`という構文を使ってスタック上に一時バッファーを確保できる。

#### 制限

`stackalloc`の性質上、以下の制限が掛かる

- あまり大きなサイズで確保できない
  - スタックオーバーフローが簡単に起きる
  - corefx や coreclr 内を見ている感じでは、せいぜい1Kバイト程度までしか`stackalloc`を使っていない
- `yield return`をまたげない
  - 「配列→配列」だからこそ使えるのであって、`IEnumerable<T>`を返すような作りには使えない

ただし、これらの制限は、以下のような「`stackalloc`と配列の分岐」を行えば回避できる。

```cs
Span<T> buffer = len < 1024 ? (Span<T>)stackalloc T[len] : (Span<T>)new T[len]
```

### 値型ジェネリクス(`EqualityComparer`のインライン化)

ハッシュテーブル内では、キーのハッシュ値取得や等値判定のために`IEqualityComparer<T>`が必要になる。

ここで、例えば以下のようなcomparerがあったとする。

```cs
public struct StructEquatableComparer<T> : IEqualityComparer<T>
    where T : IEquatable<T>
{
    public bool Equals(T x, T y) => x.Equals(y);
    public int GetHashCode(T obj) => obj.GetHashCode();
}
```

これに対して、まず、普通に以下のような呼び方をする。

```cs
public class HashSet<T>
{
    IEqualityComparer<T> _comparer;

    public bool Contains(T key)
    {
        ...
        if (_comparer.Equals(key, bucket.Key))
        ...
    }
}
```

この場合、以下のようなコストがかかる。

- `_comparer` にインスタンスを持つコスト
  - `Dictionary`のサイズが増える
    - 元々フィールドが多い型であれば微々たるコスト増加なものの
    - 「`Distinct`用の`HashSet`」の場合、フィールドは配列ただ1つで済むので、たった1フィールド増えるだけでもサイズが倍
  - `StructEquatableComparer`は構造体なので、インターフェイスを介するとボックス化する
- インターフェイスのメンバーの仮想呼び出し
  - この書き方だと仮想呼び出しになるので、インライン化されない
  - `int`等のように、`Equals`の中身が単純なものの場合、インライン化の有無でパフォーマンスが劇的に変わる

そこで、以下のような実装を考える。

```cs
public class HashSet<T, TComparer>
    TComparer : struct, IEqualityComparer<T>
{
    public bool Contains(T key)
    {
        ...
        if (default(TComparer).Equals(key, bucket.Key))
        ...
    }
}
```

.NETのジェネリクスでは、ジェネリック型引数が構造体の場合、型ごとに展開されて、仮想呼び出しが消えるような最適化が掛かる。
インライン化も掛かるようになるため、`int`等に対する`default(TComparer).Equals`呼び出しは劇的に速くなる。

#### 制限

他の最適化と違って、この最適化は引数・戻り値に対しての制限が一切かからない。
配列でなくてもいいし、長さは未知でも、`yield return`を使っても構わない。

ただ、型の書き方がかなり面倒になる。
例えば、通常の`HashSet`であれば`HashSet<string>`と書けるのに対して、
値型ジェネリクスを使ったものは`HashSet<string, StructEquatableComparer<string>>`と書かなくてはならず、かなり煩雑になる。

また、ジェネリック型引数の型推論も効きにくくなる。
例えば`Distinct`であれば、`System.Linq.Enumerable`のものは`source.Distinct()`だけでいいのに対して、
値型ジェネリクスを使ったものは`source.Distinct<string, StructEquatableComparer<string>>()`と書かなくてはならない。