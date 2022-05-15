using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lucene.Net.QueryParsers;

namespace Common.Utils
{
    /// <summary>
    /// Static methods for boolean query parsing
    /// </summary>
    public class BooleanQueryParser
    {
        /// <summary>
        /// Parse boolean query
        /// allower operators: AND, OR, NOT, (, )
        /// operator priority: (, ), NOT, AND, OR
        /// NOT ParserNode has only one child, which is always the LeftChild
        /// </summary>
        /// <param name="queryString">query to parse</param>
        /// <returns>Binary expression tree</returns>
        public static ParserNode ParseQuery(string queryString)
        {
            var tokens = Tokenize(queryString);
            var postfix = InfixToPostfix(tokens);
            var tree = PostfixToTree(postfix);
            return tree;
        }

        /// <summary>
        /// Convert postfix notation to binary expression tree
        /// </summary>
        /// <param name="postfix">postfix token list</param>
        /// <returns>binary expression tree</returns>
        private static ParserNode PostfixToTree(List<string> postfix)
        {
            Stack<ParserNode> stack = new();
            ParserNode t1, t2, temp;

            for (int i = 0; i < postfix.Count; i++)
            {
                if (!IsOperator(postfix[i]))
                {
                    temp = new ParserNode(postfix[i]);
                    stack.Push(temp);
                }
                else
                {
                    temp = new ParserNode(postfix[i]);

                    if (temp.Type == ParserNode.NodeType.NOT)
                    {
                        temp.LeftChild = stack.Pop();
                    }
                    else
                    {
                        temp.RightChild = stack.Pop();
                        temp.LeftChild = stack.Pop();
                    }

                    stack.Push(temp);
                }
            }

            return stack.Pop();
        }

        /// <summary>
        /// Converts infix to postfix
        /// </summary>
        /// <param name="infix">infix list of tokens</param>
        /// <returns>postfix list of tokens</returns>
        /// <exception cref="InvalidOperationException"></exception>
        private static List<string> InfixToPostfix(List<string> infix)
        {
            List<string> result = new();
            Stack<string> stack = new();

            for (int i = 0; i < infix.Count; i++)
            {
                var token = infix[i];

                if (!IsOperator(token))
                {
                    result.Add(token);
                }
                else if (token == "(")
                {
                    stack.Push(token);
                }
                else if (token == ")")
                {
                    while (stack.Count > 0 && stack.Peek() != "(")
                    {
                        result.Add(stack.Pop());
                    }

                    if (stack.Count > 0 && stack.Peek() != "(")
                    {
                        throw new InvalidOperationException("Malformed query");
                    }
                    else
                    {
                        stack.Pop();
                    }
                }
                else
                {
                    while (stack.Count > 0 && Precedence(token) <= Precedence(stack.Peek()))
                    {
                        result.Add(stack.Pop());
                    }
                    stack.Push(token);
                }
            }

            while (stack.Count > 0)
            {
                result.Add(stack.Pop());
            }

            return result;
        }

        /// <summary>
        /// Operator precedence
        /// </summary>
        /// <param name="op"></param>
        /// <returns></returns>
        private static int Precedence(string op)
        {
            switch (op)
            {
                case "NOT":
                    return 2;
                case "AND":
                    return 1;
                case "OR":
                    return 0;
            }

            return -1;
        }

        private static bool IsOperator(string token)
        {
            return token == "AND" || token == "OR" || token == "NOT" || token == "(" || token == ")";
        }

        /// <summary>
        /// Tokenizes the query into tokens and adds default operators
        /// </summary>
        /// <param name="queryString"></param>
        /// <returns></returns>
        private static List<string> Tokenize(string queryString)
        {
            List<string> tokens = new List<string>();
            StringBuilder sb = new StringBuilder();

            for (int i = 0; i < queryString.Length; i++)
            {
                char c = queryString[i];
                switch (c)
                {
                    case ' ':
                        // On space
                        if (sb.Length > 0)
                        {
                            tokens.Add(sb.ToString());
                            sb.Clear();
                        }
                        break;
                    case '(':
                        if (sb.Length > 0)
                        {
                            tokens.Add(sb.ToString());
                            sb.Clear();
                            sb.Append(c);
                        }
                        else
                        {
                            tokens.Add(c.ToString());
                        }
                        break;
                    case ')':
                        if (sb.Length > 0)
                        {
                            tokens.Add(sb.ToString());
                            sb.Clear();
                            sb.Append(c);
                        }
                        else
                        {
                            tokens.Add(c.ToString());
                        }
                        break;
                    case 'A':
                        if (sb.Length > 0)
                        {
                            sb.Append(c);
                        }
                        else
                        {
                            if (i + 2 < queryString.Length)
                            {
                                if ((queryString[i + 1] == 'N' && queryString[i + 2] == 'D') && IsEndOfExpression(queryString, i + 2))
                                {
                                    tokens.Add("AND");
                                    i += 2;
                                }
                                else
                                {
                                    sb.Append(c);
                                }
                            }
                            else
                            {
                                sb.Append(c);
                            }
                        }
                        break;
                    case 'O':
                        if (sb.Length > 0)
                        {
                            sb.Append(c);
                        }
                        else
                        {
                            if (i + 1 < queryString.Length)
                            {
                                if (queryString[i + 1] == 'R' && IsEndOfExpression(queryString, i + 1))
                                {
                                    tokens.Add("OR");
                                    i++;
                                }
                                else
                                {
                                    sb.Append(c);
                                }
                            }
                            else
                            {
                                sb.Append(c);
                            }
                        }
                        break;
                    case 'N':
                        if (sb.Length > 0)
                        {
                            sb.Append(c);
                        }
                        else
                        {
                            if (i + 2 < queryString.Length)
                            {
                                if ((queryString[i + 1] == 'O' && queryString[i + 2] == 'T') && IsEndOfExpression(queryString, i + 2))
                                {
                                    tokens.Add("NOT");
                                    i += 2;
                                }
                                else
                                {
                                    sb.Append(c);
                                }
                            }
                            else
                            {
                                sb.Append(c);
                            }
                        }
                        break;
                    default:
                        sb.Append(c);
                        break;
                }
            }

            if (sb.Length > 0)
            {
                tokens.Add(sb.ToString());
            }

            // Add default operators
            List<string> final_tokens = new();
            for (int i = 0; i < tokens.Count; i++)
            {
                final_tokens.Add(tokens[i]);
                if (i == tokens.Count - 1)
                {
                    break;
                }
                if (!IsNextTokenOperator(tokens, i) && !IsNextTokenOperator(tokens, i - 1))
                {
                    final_tokens.Add("OR");
                }
            }

            return final_tokens;
        }

        private static bool IsNextTokenOperator(List<string> tokens, int i)
        {
            return !(tokens[i + 1] != "AND" && tokens[i + 1] != "OR" && tokens[i + 1] != "NOT" && tokens[i + 1] != ")" && tokens[i + 1] != "(");
        }

        /// <summary>
        /// Checks if the next character is parantheses or a space, marking an end of expression
        /// </summary>
        /// <param name="s">query</param>
        /// <param name="i">char index</param>
        /// <returns></returns>
        private static bool IsEndOfExpression(string s, int i)
        {
            if (i == s.Length - 1) return true;
            if (s[i + 1] == ')' || s[i + 1] == '(' || s[i + 1] == ' ') return true;
            return false;
        }


    }

    public class ParserNode
    {
        public ParserNode(string text)
        {
            Text = text;
            switch (text)
            {
                case "OR":
                    Type = NodeType.OR;
                    break;
                case "AND":
                    Type = NodeType.AND;
                    break;
                case "NOT":
                    Type = NodeType.NOT;
                    break;
                default:
                    Type = NodeType.TERM;
                    break;
            }
        }

        public string Text { get; set; } = "";

        public ParserNode LeftChild { get; set; }
        public ParserNode RightChild { get; set; }

        public NodeType Type { get; set; }

        public enum NodeType
        {
            AND,
            OR,
            NOT,
            TERM
        }

    }
}
