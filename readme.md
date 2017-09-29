# Optimized LINQ subset dedicated to array

## Background

One day when I was looking at our codes, about 80% of the scene using LINQ uses array as an argument, and calls `ToArray` instantly. In many case, We do not need the versatility as much as `System.Enumerable`, so I want optimized LINQ subset dedicated to array → array situation.

Therefore, I implemented such LINQ subset with the following assumption.

- Dedicated to array
- Not assume pipelining<sup>※1</sup>
- Length, or at least the upper limit<sup>※2</sup> of length, is known
- The length is short enough<sup>※3</sup>

(Though this restriction is tremendously strong, the rate of speeding up is (commonly) 30% ～ (very rarely) 400%. So I'm not sure it is useful or not ...)

<sup>※1</sup>
A multistage query such as `Select().Where.Select().OrderBy()...`. Array to array optimization reduces an overhead. However, allocating array in each pipeline stage has much high computational costs than the overhead.

<sup>※2</sup>
For example, in the case of 'Where' or 'Distinct', the exact length of the output is not known but the maximum length could be known. The output is always shorter than the input.

<sup>※3</sup>
This is a limit necessary to allocate temporary buffer with `stackalloc`. `stackalloc`ing big size memory easily raises stack overflows. In the near future with C# 7.2, this issue will be avoided with `Span<T> buffer = len < LimitSize ? (Span<T>)stackalloc T[len] : (Span<T>)new T[len]`.

## Optimization method

### Dedicated to array

In general, the `foreach` statement is expanded as follows.

Before:

```cs
foreach (var x in data)
{
}
```

After (in general):


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

For an array, however, this is expanded as follows.

After (for an array):

```cs
for (int i = 0; i < data.Length; i++)
{
    var x = data[i];
}
```

This code is faster than the general `IEnumerable<T>` code. As described later, two costs - allocating `IEnumerator` and range check of array - disappear.

#### Restriction

For this optimization, it really must be dedicated to array.

(In the other optimization, it is sufficient if the length is known. Thus `IList` or `IEnumerable` + length can be used.)

#### (Supplement) `IEnumerator` allocation

In the general case of the `foreach` statement, `GetEnumerator` allocates an `IEnumearator` instance.

`GetEnumerator` in some classes such as `List<T>` returns a `struct` enumerator, but the enumerator is "boxed" if calling the `GetEnumerator` via `IEnumerable<T>` interface. This causes an heap allocation - small but not trivial cost.

#### (Supplement) Range check of array

Normally, The JIT compiler inserts a range check for each index access to array. It is indispensable to prevent buffer overrun vulnerability but costs a considerable amount.

However, when writing a `for` statement like the following, the range check in the index access `x[i]` is eliminated in JIT optimization process.

```cs
for ( int i = 0 ; i <a. Length; i ++)
{
    var x = a [i];
} 
```

For `IList<T>` or `IReadOnlyList<T>`, such an optimization will not work even if you write the same `for` statement

### Make the length known

In general, `ToArray` operation for `IEnumerable<T>` (size unknown) needs temporary buffer which is resized on demand.

On the other hand, if the length is known at the biginning, an array allocation needs only one time. 

#### Restriction

As well as array, `IList<T>` can also be used because its length is known. However, `IEnumerable<T>` can not in general.

### Dedicated implementation of hash table

Implementations such as `Distinct` and `GroupBy` use a hash table internally.

There are some points that a hash table can be optimized rather than using these standard implementations, `Dictionary<TKey, TValue>` and `HashSet<T>`.

- `Remove` method is not needed for `Distinct` and `GroupBy`
  - Hash table algorithm can be much simpler if `Remove` method is not needed
- If the length is known, there is no need to resize the temporary buffer

#### Restriction

There is no particular restriction in this optimization.

### Temporary buffer with `stackalloc`

Temporary buffer in the hash table is used only in the method.

It is a waste to use the managed heap for those whose lifetime is definite. The buffer can be allocated on the stack with `stackalloc` operator.

#### Restriction

`stackalloc` operator has the following restrictions.

- Can not allocate too large size
  - Stack overflow easily happens
  - Most codes in corefx and coreclr allocate memory with `stackalloc` by 1 Kbytes or smaller.
- Can not run over `yield return` and `await` expressions


###  Value type generics (inlining `EqualityComparer`)

A hash table needs `IEqualityComparer<T>` to obtain hash values and compare keys.

Here, for example, suppose that there is a comparer such as the following.

```cs
public struct StructEquatableComparer<T> : IEqualityComparer<T>
    where T : IEquatable<T>
{
    public bool Equals(T x, T y) => x.Equals(y);
    public int GetHashCode(T obj) => obj.GetHashCode();
}
```

First, if you call it as follows

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

This has ther following costs.

- having an instance in `_comparer`
  - The size of the `Dictionary` increases
  - The comparer instance is boxed because `StructEquatableComparer` is a `struct`
- Virtual call of interface members
  - Virtual call prevent method inlining in general
  - Penalty of preventing method inlining is very large especially for primitives such as `int`

Therefore, we consider the following implementation.

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

In .NET generics, when the type parameter is a `struct`, generic types are expanded for each actual type argument. As a result, virtual call disappears and the method call can be inlined. The `default(TComparer).Equals` call becomes dramatically faster.

#### Restriction

This optimization does not restrict in terms of types.


However, codes with the optimizaion is very ugly. For example, you have to write `HashSet<string, StructEquatableComparer<string>>` (very complicated) for the optimized type, while you can write `HashSet<string>` for normal one.

Also, type inference of generic type parameter becomes less effective. For example, `Distinct` call is `source.Distinct<string, StructEquatableComparer<string>>()` for the optimized one but `source.Distinct()` for normal one.
