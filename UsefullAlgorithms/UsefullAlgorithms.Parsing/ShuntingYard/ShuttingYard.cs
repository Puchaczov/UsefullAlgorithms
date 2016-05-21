﻿using System;
using System.Collections;
using System.Collections.Generic;

namespace UsefullAlgorithms.Parsing.ExpressionParsing
{
    public static class StackHelper
    {
        public static bool IsEmpty<T>(this Stack<T> stack) => stack.Count == 0;
    }


    /// <summary>
    /// Implementation based on https://en.wikipedia.org/wiki/Shunting-yard_algorithm
    /// </summary>
    /// <typeparam name="TExpression"></typeparam>
    /// <typeparam name="TToken"></typeparam>
    public abstract class ShuntingYard<TExpression, TToken>
        where TExpression : IEnumerable
        where TToken : IComparable, IComparable<TToken>, IEquatable<TToken>
    {

        protected readonly Dictionary<TToken, PrecedenceAssociativity> operators;

        public Dictionary<TToken, int> FunctionArgsCount { get; }

        public ShuntingYard()
        {
            operators = new Dictionary<TToken, PrecedenceAssociativity>();
            FunctionArgsCount = new Dictionary<TToken, int>();
        }

        public ShuntingYard(params KeyValuePair<TToken, PrecedenceAssociativity>[] rules)
        {
            foreach(var rule in rules)
            {
                this.operators.Add(rule.Key, rule.Value);
            }
        }

        public abstract TToken[] Parse(TExpression expression);

        protected TToken[] InfixToPostfix(IEnumerable<TToken> expression)
        {
            FunctionArgsCount.Clear();

            List<TToken> output = new List<TToken>();
            Stack<TToken> stack = new Stack<TToken>();

            foreach (var token in expression)
            {
                if (IsSkippable(token))
                    continue;

                if (IsOperator(token))
                {
                    while (!stack.IsEmpty() && IsOperator(stack.Peek()))
                    {
                        if (
                            (IsAssociative(token, Associativity.Left) && TestPrecedence(token, stack.Peek()) <= 0) ||
                            IsAssociative(token, Associativity.Right) && TestPrecedence(token, stack.Peek()) < 0)
                        {
                            output.Add(stack.Pop());
                            continue;
                        }
                        break;
                    }
                    stack.Push(token);
                }
                else if (IsLeftParenthesis(token))
                {
                    stack.Push(token);
                }
                else if (IsRightParenthesis(token))
                {
                    while (!stack.IsEmpty() && !IsLeftParenthesis(stack.Peek()))
                    {
                        output.Add(stack.Pop());
                    }
                    stack.Pop();
                    if(!stack.IsEmpty() && IsWord(stack.Peek()) && !IsOperator(stack.Peek()))
                    {
                        output.Add(stack.Pop());
                    }
                }
                else if (IsWord(token) && !IsOperator(token))
                {
                    stack.Push(token);
                }
                else if(IsComma(token))
                {
                    while(!stack.IsEmpty() && !IsLeftParenthesis(stack.Peek()))
                    {
                        output.Add(stack.Pop());
                    }
                }
                else
                {
                    output.Add(token);
                }
            }
            while (!stack.IsEmpty())
            {
                output.Add(stack.Pop());
            }

            return output.ToArray();
        }

        internal abstract bool IsComma(TToken token);
        internal abstract bool IsWord(TToken token);
        protected abstract bool IsSkippable(TToken token);

        protected abstract bool IsRightParenthesis(TToken token);
        protected abstract bool IsLeftParenthesis(TToken token);

        protected bool IsAssociative(TToken token, Associativity associativity) => IsOperator(token) && operators[token].Associativity == associativity;
        protected bool IsOperator(TToken token) => operators.ContainsKey(token);

        protected int TestPrecedence(TToken token1, TToken token2)
        {
            if (IsOperator(token1) && IsOperator(token2))
            {
                return operators[token1].Weight - operators[token2].Weight;
            }
            throw new ArgumentException("One of arguments isn't operator");
        }
    }
}
