// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Subroute.Core.Compiler.Intellisense.KeywordRecommenders
{
    internal class ContinueKeywordRecommender : AbstractSyntacticSingleKeywordRecommender
    {
        public ContinueKeywordRecommender()
            : base(SyntaxKind.ContinueKeyword)
        {
        }

        protected override bool IsValidContext(int position, CSharpSyntaxContext context, CancellationToken cancellationToken)
        {
            if (!context.IsStatementContext)
            {
                return false;
            }

            // allowed if we're inside a loop construct.

            var leaf = context.LeftToken;
            foreach (var v in leaf.GetAncestors<SyntaxNode>())
            {
                if (v.IsAnyLambdaOrAnonymousMethod())
                {
                    // if we hit a lambda while walking up, then we can't
                    // 'continue' any outer loops.
                    return false;
                }

                if (v.IsContinuableConstruct())
                {
                    return true;
                }
            }

            return false;
        }
    }
}
