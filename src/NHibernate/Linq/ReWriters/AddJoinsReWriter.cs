using NHibernate.Linq.Visitors;
using Remotion.Linq;
using Remotion.Linq.Clauses;

namespace NHibernate.Linq.ReWriters
{
	internal interface IIsEntityDecider
	{
		bool IsEntity(System.Type type);
		bool IsIdentifier(System.Type type, string propertyName);
	}

	public class AddJoinsReWriter : QueryModelVisitorBase, IIsEntityDecider
	{
		private readonly ISessionFactory _sessionFactory;
		private readonly SelectAndOrderByJoinDetector _selectAndOrderByJoinDetector;
		private readonly WhereJoinDetector _whereJoinDetector;

		private AddJoinsReWriter(ISessionFactory sessionFactory, QueryModel queryModel)
		{
			_sessionFactory = sessionFactory;
			var joiner = new Joiner(queryModel);
			_selectAndOrderByJoinDetector = new SelectAndOrderByJoinDetector(this, joiner);
			_whereJoinDetector = new WhereJoinDetector(this, joiner);
		}

		public static void ReWrite(QueryModel queryModel, ISessionFactory sessionFactory)
		{
			new AddJoinsReWriter(sessionFactory, queryModel).VisitQueryModel(queryModel);
		}

		public override void VisitSelectClause(SelectClause selectClause, QueryModel queryModel)
		{
			_selectAndOrderByJoinDetector.Transform(selectClause);
		}

		public override void VisitOrdering(Ordering ordering, QueryModel queryModel, OrderByClause orderByClause, int index)
		{
			_selectAndOrderByJoinDetector.Transform(ordering);
		}

		public override void VisitResultOperator(ResultOperatorBase resultOperator, QueryModel queryModel, int index)
		{
			_selectAndOrderByJoinDetector.Transform(resultOperator);
		}

		public override void VisitWhereClause(WhereClause whereClause, QueryModel queryModel, int index)
		{
			_whereJoinDetector.Transform(whereClause);
		}

		public bool IsEntity(System.Type type)
		{
			return _sessionFactory.GetClassMetadata(type) != null;
		}

		public bool IsIdentifier(System.Type type, string propertyName)
		{
			var metadata = _sessionFactory.GetClassMetadata(type);
			return metadata != null && propertyName.Equals(metadata.IdentifierPropertyName);
		}
	}
}
