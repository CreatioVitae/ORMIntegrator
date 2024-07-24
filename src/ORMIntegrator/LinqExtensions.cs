using System.Linq;
using System.Linq.Expressions;

namespace ORMIntegrator;
public static class LinqExtensions {
    public static IQueryable<TSource> WhereIf<TSource>(this IQueryable<TSource> source, bool Condition, Expression<Func<TSource, bool>> Predicate) =>
        Condition
            ? source.Where(Predicate)
            : source;
}
