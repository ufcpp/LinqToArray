using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace LinqToArrayUnitTest
{
    public class TestData
    {
        static List<int[]> intData = new List<int[]>();
        static List<string[]> strData = new List<string[]>();

        static TestData()
        {
            const int N = 1000;
            var r = new Random(0);

            intData.Add(new int[0]);
            intData.Add(new[] { 1 });
            intData.Add(Enumerable.Range(0, N).Select(_ => 1).ToArray());
            intData.Add(Enumerable.Range(0, N).Select(_ => r.Next(0, 10)).ToArray());
            intData.Add(Enumerable.Range(0, N).Select(_ => r.Next(0, 100)).ToArray());
            intData.Add(Enumerable.Range(0, N).ToArray());

            strData.Add(new string[0]);
            strData.Add(new[] { "aaaa" });

            var text = @"
O, speak again, bright angel! For thou art As glorious to this night, being o'er my head, As is a wingèd messenger of heaven Unto the white, upturnèd, wondering eyes Of mortals that fall back to gaze on him When he bestrides the lazy-puffing clouds And sails upon the bosom of the air.
O Romeo, Romeo! Wherefore art thou Romeo? Deny thy father and refuse thy name. Or, if thou wilt not, be but sworn my love, And I’ll no longer be a Capulet.
Shall I hear more, or shall I speak at this?
'Tis but thy name that is my enemy. Thou art thyself, though not a Montague. What’s Montague? It is nor hand, nor foot, Nor arm, nor face, nor any other part Belonging to a man. O, be some other name! What’s in a name? That which we call a rose By any other word would smell as sweet. So Romeo would, were he not Romeo called, Retain that dear perfection which he owes Without that title. Romeo, doff thy name, And for that name, which is no part of thee Take all myself.
I take thee at thy word. Call me but love, and I’ll be new baptized. Henceforth I never will be Romeo.
What man art thou that, thus bescreened in night, So stumblest on my counsel?
She speaks. Oh, speak again, bright angel. You are as glorious as an angel tonight. You shine above me, like a winged messenger from heaven who makes mortal men fall on their backs to look up at the sky, watching the angel walking on the clouds and sailing on the air.
Oh, Romeo, Romeo, why do you have to be Romeo? Forget about your father and change your name. Or else, if you won’t change your name, just swear you love me and I’ll stop being a Capulet.
Should I listen for more, or should I speak now?
It’s only your name that’s my enemy. You’d still be yourself even if you stopped being a Montague. What’s a Montague anyway? It isn’t a hand, a foot, an arm, a face, or any other part of a man. Oh, be some other name! What does a name mean? The thing we call a rose would smell just as sweet if we called it by any other name. Romeo would be just as perfect even if he wasn’t called Romeo. Romeo, lose your name. Trade in your name—which really has nothing to do with you—and take all of me in exchange.
I trust your words. Just call me your love, and I will take a new name. From now on I will never be Romeo again.
Who are you? Why do you hide in the darkness and listen to my private thoughts?";

            var words = text.Split(' ', ',', '.', '?', '!').Where(s => !string.IsNullOrWhiteSpace(s)).ToArray();
            strData.Add(words);
        }

        public static IReadOnlyList<int[]> IntData => intData;
        public static IReadOnlyList<string[]> StrData => strData;
    }

    public class TestSequence1 : IEnumerable<object[]>
    {
        public IEnumerator<object[]> GetEnumerator()
        {
            foreach (var x in TestData.IntData)
            {
                yield return new object[] { x };
            }
            foreach (var x in TestData.StrData)
            {
                yield return new object[] { x };
            }
        }

        IEnumerator IEnumerable.GetEnumerator() => throw new NotImplementedException();
    }
}
