using DotMpi;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection.Metadata.Ecma335;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using YamlDotNet.Core.Tokens;

namespace DotOrmLib
{
    public interface IWhereClauseBuilder<T>
    {
        IWhereClauseBuilder<T> And(Expression<Func<T, bool>> expression);
        IWhereClauseBuilder<T> Or(Expression<Func<T, bool>> expression);
        (string WhereClause, Dictionary<string, object?> Parameters) Build();
        Task<List<T>> ToList();
    }

    public class WhereClauseBuilder<T> : IWhereClauseBuilder<T>
        where T : class
    {
        private readonly List<string> _whereConditions;
        private DotOrmRepo<T> repo;
        private Dictionary<string, object?> parameters;
        public async Task<List<T>> ToList()
        {
            return await repo.Get(this);
        }


        public WhereClauseBuilder(Expression<Func<T, bool>> expression, DotOrmRepo<T> repo)
        {
            this.repo = repo;
            this.parameters = new();

            _whereConditions = new List<string>();
            string condition = GetCondition(expression, "AND");
            _whereConditions.Add(condition);


        }

        public IWhereClauseBuilder<T> And(Expression<Func<T, bool>> expression)
        {
            string condition = GetCondition(expression, "AND");
            _whereConditions.Add(condition);
            return this;
        }

        public IWhereClauseBuilder<T> Or(Expression<Func<T, bool>> expression)
        {
            string condition = GetCondition(expression, "OR");
            _whereConditions.Add(condition);
            return this;
        }

        private string GetCondition(Expression<Func<T, bool>> expression, string logicalOperator)
        {
            var visitor = new ExpressionVisitor<T>(this);
            visitor.Visit(expression.Body);

            string condition = visitor.ToString();
            if (!string.IsNullOrWhiteSpace(condition))
            {
                return _whereConditions.Count > 0 ? $"{logicalOperator} {condition}" : condition;
            }

            return string.Empty;
        }

        public (string WhereClause, Dictionary<string, object?> Parameters) Build()
        {

            return (WhereClause: string.Join(" ", _whereConditions), Parameters: parameters);
        }
        public FilterRequest BuildFilter()
        {
            return new FilterRequest(Build());
        }
        private class ExpressionVisitor<T> : ExpressionVisitor
            where T : class
        {
            private readonly StringBuilder _sb = new StringBuilder();
            private DotOrmRepo<T> repo;
            private WhereClauseBuilder<T> builder;



            public ExpressionVisitor(WhereClauseBuilder<T> whereClauseBuilder)
            {
                this.builder = whereClauseBuilder;
                this.repo = builder.repo;
            }

            protected override Expression VisitBinary(BinaryExpression node)
            {
                _sb.Append("(");
                Visit(node.Left);

                _sb.Append($" {GetOperator(node.NodeType, node.Right)} ");
                Visit(node.Right);
                _sb.Append(")");
                return node;
            }

            protected override Expression VisitConstant(ConstantExpression node)
            {
                if (node.Value is not null)
                {
                    var name = $"@p_{builder.parameters.Count}";
                    builder.parameters.Add(name, node.Value);
                    _sb.Append(name);
                }

                return node;
            }

            protected override Expression VisitMember(MemberExpression node)
            {
                if (node.NodeType == ExpressionType.MemberAccess)
                {
                    var member = node.Member;
                    if (member.DeclaringType == typeof(T))
                    {
                        var columnName = repo.Model.TryGetColumnNameByProperty(member.Name);
                        _sb.Append($"[{columnName}]");
                        return node;
                    }
                }
                _sb.Append(node.Member.Name);
                return node;
            }

            private string GetOperator(ExpressionType nodeType, Expression rightNode)
            {
                if (nodeType == ExpressionType.Equal
                    && rightNode.NodeType == ExpressionType.Constant
                    && ((ConstantExpression)rightNode).Value == null)
                {
                    return "IS NULL";
                }
                else if (nodeType == ExpressionType.NotEqual
                    && rightNode.NodeType == ExpressionType.Constant
                    && ((ConstantExpression)rightNode).Value == null)
                {
                    return "IS NOT NULL";
                }

                switch (nodeType)
                {
                    case ExpressionType.Equal: return "=";
                    case ExpressionType.NotEqual: return "<>";
                    case ExpressionType.GreaterThan: return ">";
                    case ExpressionType.GreaterThanOrEqual: return ">=";
                    case ExpressionType.LessThan: return "<";
                    case ExpressionType.LessThanOrEqual: return "<=";
                    case ExpressionType.AndAlso: return "AND";
                    case ExpressionType.OrElse: return "OR";
                    default: throw new NotSupportedException($"Operator {nodeType} is not supported.");
                }
            }

            public override string ToString()
            {
                return _sb.ToString();
            }
        }
    }

    [DataContract]
    public class FilterRequest
    {
        Dictionary<string, SerializableValue> Parameters { get;  set; }

        public FilterRequest() { }
        public FilterRequest(int skip = 0, int take = 100)
        {
            Skip = skip;
            Take = take;
            WhereClause = string.Empty;
            this.ParameterJson = string.Empty;
            Parameters = new();
        }
        public FilterRequest(string whereClause, Dictionary<string, object?> parameters, int skip = 0, int take = 100)
        {
            Skip = skip;
            Take = take;
            WhereClause = whereClause;
            Parameters = new();
            this.ParameterJson = string.Empty;
            addParameters(parameters);
        }

        private void addParameters(Dictionary<string, object?> parameters)
        {
            parameters.ToList().ForEach(x =>
            {
                if (x.Value is SerializableValue serializable)
                    Parameters.Add(x.Key, serializable);
                else
                    Parameters.Add(x.Key, new SerializableValue(x.Value));
            });
            this.ParameterJson = JsonConvert.SerializeObject(Parameters);
        }

        public FilterRequest((string WhereClause, Dictionary<string, object?> Parameters) value)
        {
            this.WhereClause = value.WhereClause;
            this.Parameters = new();
            this.ParameterJson = string.Empty;
            addParameters(value.Parameters);
            

        }

        [DataMember(Order = 1)]
        public int Skip { get; set; } = 0;
        [DataMember(Order = 2)]
        public int Take { get; set; } = 100;
        [DataMember(Order = 3)]
        public string WhereClause { get; set; }
        [DataMember(Order = 4)]
        public string ParameterJson { get; set; }
 
    }
}
