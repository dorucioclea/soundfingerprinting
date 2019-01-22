﻿namespace SoundFingerprinting.LCS
{
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Security.Principal;
    using SoundFingerprinting.Query;

    using Math = System.Math;
        
    internal class LongestIncreasingTrackSequence : ILongestIncreasingTrackSequence
    {
        private const float AllowedMismatchLength = 1.48f;

        public List<List<MatchedWith>> FindAllIncreasingTrackSequences(IEnumerable<MatchedWith> matches)
        {
            var matchedWiths = new List<List<MatchedWith>>();
            var list = matches.OrderBy(match => match.QueryAt).ToList();
            while (list.Any())
            {
                var orderedByQueryAt = list.ToArray();
                MaxAt[] maxLength = BuildMaxLengthIndexArray(orderedByQueryAt, out var max, out var maxIndex);
                var longestSequence = FindLongestSequence(orderedByQueryAt, maxLength, max, maxIndex).ToList();
                matchedWiths.Add(longestSequence);
                list = list.Except(longestSequence)
                           .OrderBy(match => match.QueryAt)
                           .ToList();
            }

            return matchedWiths;
        }

        private static IEnumerable<MatchedWith> FindLongestSequence(MatchedWith[] matches, MaxAt[] maxLength, int max, int maxIndex)
        {
            var lis = new Stack<MatchedWith>();
            lis.Push(matches[maxIndex]);
            max--;
            
            for (int i = maxIndex - 1; i >= 0; --i)
            {
                if (maxLength[i].Length == max)
                {
                    var prev = lis.Peek();
                    if (Math.Abs(prev.ResultAt - maxLength[i].ResultAt) <= AllowedMismatchLength)
                    {
                        lis.Push(matches[i]);
                        max--;
                    }
                }
            }

            while (lis.Any())
            {
                yield return lis.Pop();
            }
        }

        private static MaxAt[] BuildMaxLengthIndexArray(IReadOnlyList<MatchedWith> matches, out int max, out int maxIndex)
        {
            var maxLength = new MaxAt[matches.Count];

            for (int i = 0; i < maxLength.Length; ++i)
            {
                maxLength[i] = new MaxAt(0, matches[i].ResultAt);
            }
            
            max = 0;
            maxIndex = 0;
            
            for (int i = 1; i < matches.Count; ++i)
            {
                for (int j = 0; j < i; ++j)
                {
                    if (matches[j].ResultAt < matches[i].ResultAt && maxLength[j].Length + 1 > maxLength[i].Length)
                    {
                        float queryAt = Math.Abs(matches[i].QueryAt - matches[j].QueryAt);
                        float resultAt = Math.Abs(matches[i].ResultAt - matches[j].ResultAt);
                        
                        if (queryAt <= AllowedMismatchLength && resultAt <= AllowedMismatchLength)
                        {
                            var maxAt = new MaxAt(maxLength[j].Length + 1, matches[i].ResultAt);
                            maxLength[i] = maxAt;
                            if (maxLength[i].Length > max)
                            {
                                max = maxLength[i].Length;
                                maxIndex = i;
                            }
                        }
                    }
                }
            }

            return maxLength;
        }

        private static bool alreadyContains(MaxAt maxAt, MaxAt[] lengths)
        {
            for (int i = 0; i < lengths.Length; ++i)
            {
                if (lengths[i].Length == maxAt.Length && Math.Abs(lengths[i].ResultAt - maxAt.ResultAt) < 0.000001d)
                {
                    return true;
                }
            }

            return false;
        }
        

        private struct MaxAt
        {
            public MaxAt(int length, double resultAt)
            {
                Length = length;
                ResultAt = resultAt;
            }

            public int Length { get; }

            public double ResultAt { get; }
        }
    }
}
