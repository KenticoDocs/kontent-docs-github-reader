﻿using System;
using System.Collections.Generic;
using GithubService.Models;
using NUnit.Framework.Constraints;

namespace GithubService.Tests.Common.Comparers
{
    public class CodeFragmentComparer : IEqualityComparer<CodeFragment>
    {
        public bool Equals(CodeFragment x, CodeFragment y)
        {
            if (ReferenceEquals(x, y)) return true;
            if (ReferenceEquals(x, null)) return false;
            if (ReferenceEquals(y, null)) return false;
            if (x.GetType() != y.GetType()) return false;

            return string.Equals(x.Identifier, y.Identifier) && 
                string.Equals(x.Content, y.Content) && 
                x.Language == y.Language && 
                x.Platform == y.Platform;
        }

        public int GetHashCode(CodeFragment obj)
        {
            unchecked
            {
                var hashCode = (obj.Identifier != null ? obj.Identifier.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (obj.Content != null ? obj.Content.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (obj.Language != null ? obj.Language.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (obj.Platform != null ? obj.Platform.GetHashCode() : 0);

                return hashCode;
            }
        }
    }

    public static class CodeFragmentComparerWrapper
    {
        private static Lazy<CodeFragmentComparer> Lazy => new Lazy<CodeFragmentComparer>();

        private static CodeFragmentComparer Comparer => Lazy.Value;

        public static SomeItemsConstraint UsingCodeFragmentComparer(this SomeItemsConstraint constraint) =>
            constraint.Using(Comparer);
    }
}
