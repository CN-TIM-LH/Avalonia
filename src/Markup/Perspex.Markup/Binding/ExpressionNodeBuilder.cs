﻿// Copyright (c) The Perspex Project. All rights reserved.
// Licensed under the MIT license. See licence.md file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Perspex.Markup.Binding
{
    internal class ExpressionNodeBuilder
    {
        public static ExpressionNode Build(string expression)
        {
            if (string.IsNullOrWhiteSpace(expression))
            {
                throw new ArgumentException("'expression' may not be empty.");
            }

            var syntaxTree = CSharpSyntaxTree.ParseText(expression, new CSharpParseOptions(kind: SourceCodeKind.Interactive));
            var syntaxRoot = syntaxTree.GetRoot();
            var syntax = syntaxRoot.ChildNodes().SingleOrDefault()?.ChildNodes()?.SingleOrDefault();

            if (syntax != null)
            {
                return Build(expression, syntax, null);
            }
            else
            {
                throw new Exception($"Invalid expression: {expression}");
            }
        }

        private static ExpressionNode Build(string expression, SyntaxNode syntax, ExpressionNode next)
        {
            var expressionStatement = syntax as ExpressionStatementSyntax;
            var identifier = syntax as IdentifierNameSyntax;
            var memberAccess = syntax as MemberAccessExpressionSyntax;
            var unaryExpression = syntax as PrefixUnaryExpressionSyntax;

            if (expressionStatement != null)
            {
                return Build(expression, expressionStatement.Expression, next);
            }
            else if (identifier != null)
            {
                next = new PropertyAccessorNode(next, identifier.Identifier.ValueText);
            }
            else if (memberAccess != null)
            {
                next = new PropertyAccessorNode(next, memberAccess.Name.Identifier.ValueText);
                next = Build(expression, memberAccess.Expression, next);
            }
            else if (unaryExpression != null && unaryExpression.Kind() == SyntaxKind.LogicalNotExpression)
            {
                next = Build(expression, unaryExpression.Operand, next);
                next = new LogicalNotNode(next);
            }
            else
            {
                throw new Exception($"Invalid expression: {expression}");
            }

            return next;
        }
    }
}
