using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.Expressions;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Microsoft.EntityFrameworkCore.Query.ResultOperators;
using Microsoft.EntityFrameworkCore.Query.Sql;
using Microsoft.EntityFrameworkCore.SqlServer.Infrastructure.Internal;
using Microsoft.EntityFrameworkCore.SqlServer.Query.Sql.Internal;
using Remotion.Linq;
using Remotion.Linq.Clauses;
using Remotion.Linq.Clauses.ResultOperators;
using Remotion.Linq.Clauses.StreamedData;
using Remotion.Linq.Parsing.Structure;
using Remotion.Linq.Parsing.Structure.IntermediateModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Work
{
    //https://www.chasingdevops.com/sql-generation-ef-core/
    //https://github.com/aspnet/EntityFrameworkCore/blob/master/src/EFCore.SqlServer/Query/Sql/Internal/SqlServerQuerySqlGenerator.cs

    public static class IQueryableExtensions
    {
        internal static readonly MethodInfo WithSqlTweaksMethodInfo
          = typeof(IQueryableExtensions).GetTypeInfo().GetDeclaredMethod(nameof(WithSqlTweaks));

        public static IQueryable<TEntity> WithSqlTweaks<TEntity>(this IQueryable<TEntity> source) where TEntity : class
        {
            return
              source.Provider is EntityQueryProvider
                ? source.Provider.CreateQuery<TEntity>(
                  Expression.Call(
                    instance: null,
                    method: WithSqlTweaksMethodInfo.MakeGenericMethod(typeof(TEntity)),
                    arguments: source.Expression))
                : source;
        }
    }

    internal class WithSqlTweaksResultOperator : SequenceTypePreservingResultOperatorBase, IQueryAnnotation
    {
        public IQuerySource QuerySource { get; set; }
        public QueryModel QueryModel { get; set; }

        public override ResultOperatorBase Clone(CloneContext cloneContext) => new WithSqlTweaksResultOperator();

        public override StreamedSequence ExecuteInMemory<T>(StreamedSequence input) => input;

        public override void TransformExpressions(Func<Expression, Expression> transformation)
        {
        }
    }

    internal class WithSqlTweaksExpressionNode : ResultOperatorExpressionNodeBase
    {
        public static readonly IReadOnlyCollection<MethodInfo> SupportedMethods = new[]
        {
            IQueryableExtensions.WithSqlTweaksMethodInfo
        };

        public WithSqlTweaksExpressionNode(MethodCallExpressionParseInfo parseInfo)
          : base(parseInfo, null, null)
        {
        }

        protected override ResultOperatorBase CreateResultOperator(ClauseGenerationContext clauseGenerationContext)
          => new WithSqlTweaksResultOperator();

        public override Expression Resolve(
          ParameterExpression inputParameter,
          Expression expressionToBeResolved,
          ClauseGenerationContext clauseGenerationContext)
          => Source.Resolve(inputParameter, expressionToBeResolved, clauseGenerationContext);
    }

    internal class CustomMethodInfoBasedNodeTypeRegistryFactory : DefaultMethodInfoBasedNodeTypeRegistryFactory
    {
        public override INodeTypeProvider Create()
        {
            RegisterMethods(WithSqlTweaksExpressionNode.SupportedMethods, typeof(WithSqlTweaksExpressionNode));

            return base.Create();
        }
    }

    internal class CustomSelectExpression : SelectExpression
    {
        public bool UseSqlTweaks { get; set; }

        public CustomSelectExpression(
          SelectExpressionDependencies dependencies,
          RelationalQueryCompilationContext queryCompilationContext) : base(dependencies, queryCompilationContext)
        {
            SetCustomSelectExpressionProperties(queryCompilationContext);
        }

        public CustomSelectExpression(
          SelectExpressionDependencies dependencies,
          RelationalQueryCompilationContext queryCompilationContext,
          string alias) : base(dependencies, queryCompilationContext, alias)
        {
            SetCustomSelectExpressionProperties(queryCompilationContext);
        }

        private void SetCustomSelectExpressionProperties(RelationalQueryCompilationContext queryCompilationContext)
        {
            if (queryCompilationContext.QueryAnnotations.Any(a => a.GetType() == typeof(WithSqlTweaksResultOperator)))
            {
                UseSqlTweaks = true;
            }
        }
    }

    internal class CustomSelectExpressionFactory : SelectExpressionFactory
    {
        public CustomSelectExpressionFactory(SelectExpressionDependencies dependencies)
          : base(dependencies)
        {
        }

        public override SelectExpression Create(RelationalQueryCompilationContext queryCompilationContext)
          => new CustomSelectExpression(Dependencies, queryCompilationContext);

        public override SelectExpression Create(RelationalQueryCompilationContext queryCompilationContext, string alias)
          => new CustomSelectExpression(Dependencies, queryCompilationContext, alias);
    }

    internal class CustomSqlServerQuerySqlGenerator : SqlServerQuerySqlGenerator
    {
        public CustomSqlServerQuerySqlGenerator(
          QuerySqlGeneratorDependencies dependencies,
          SelectExpression selectExpression,
          bool rowNumberPagingEnabled)
          : base(dependencies, selectExpression, rowNumberPagingEnabled)
        {
        }

        public override Expression VisitSelect(SelectExpression selectExpression)
        {
            // other code left out for simplicity
            if (selectExpression is CustomSelectExpression)
            {
                if (((CustomSelectExpression)selectExpression).UseSqlTweaks)
                {
                    // Do SQL tweaks here!
                }
            }

            return base.VisitSelect(selectExpression);
        }
    }

    internal class CustomSqlServerQuerySqlGeneratorFactory : QuerySqlGeneratorFactoryBase
    {
        private readonly ISqlServerOptions _sqlServerOptions;

        public CustomSqlServerQuerySqlGeneratorFactory(
          QuerySqlGeneratorDependencies dependencies,
          ISqlServerOptions sqlServerOptions) : base(dependencies)
        {
            _sqlServerOptions = sqlServerOptions;
        }

        public override IQuerySqlGenerator CreateDefault(SelectExpression selectExpression)
          => new CustomSqlServerQuerySqlGenerator(
            Dependencies,
            selectExpression,
            _sqlServerOptions.RowNumberPagingEnabled);
    }
}
