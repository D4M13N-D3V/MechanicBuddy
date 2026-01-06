using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Carmasters.Core.Domain;
using Carmasters.Http.Api.Models;
using Dapper;



namespace Carmasters.Core.Application.Services
{
    public class PageResultQuery<DTO>
    {
        string searchText;
        private int limit;
        private int offset;
        private bool desc;
        private string searchFields;
        private string selectSql;
        private string orderby;
        private bool usePagingRestricion = true;
        private bool useWhereRestricion = true;
        private List<string> whereExpressions = new List<string>();
        private readonly IDbConnection connection;

        public PageResultQuery(IDbConnection connection)
        {
            this.connection = connection;
        }

        public PageResultQuery<DTO> FilterBy(string searchText)
        {
            this.searchText = searchText;
            return this;
        }
        public PageResultQuery<DTO> SelectSql(string selectSql)
        {
            this.selectSql = selectSql;
            return this;
        }
        public PageResultQuery<DTO> SearchFields(string searchFields)
        {
            this.searchFields = searchFields;
            return this;
        }

        public PageResultQuery<DTO> UsePagingRestriction(bool use)
        {
            this.usePagingRestricion = use;
            return this;
        }

        public PageResultQuery<DTO> UseWhereRestriction(bool use)
        {
            this.useWhereRestricion = use;
            return this;
        }

        public PageResultQuery<DTO> PageIs(string orderby, int limit, int offset, bool desc)
        {
            this.orderby = orderby;
            this.limit = limit;
            this.offset = offset;
            this.desc = desc;
            return this;
        }
        //to_tsvector(concat_ws(' ',firstname,lastname,address,phone)) @@ to_tsquery('veiko & Sindi')

        public string GetPagingRestriction()
        {
            var orderBySql = string.Empty;
            if (orderby != null)
            {
                var order = (desc ? "desc" : "asc");
                orderBySql = orderby.Contains("{0}") ? $"order by {string.Format(orderby,order)}":  $"order by {orderby} {order}";
            }
             
            var sql = $"{orderBySql} OFFSET @offset ROWS FETCH FIRST @limit ROW ONLY ";
            return sql;
        }

        public string GetWhereRestriction() 
        {
            var where = string.Empty;
            if (!string.IsNullOrWhiteSpace(searchText))
            {
                var tokens = new WildcardTokens(searchText).AllTokens();

                var restriction = string.Join(" and ", tokens.Select(word => $"{searchFields} ilike '%{word}%'"));
               
                whereExpressions.Add(restriction);
            }
            if (whereExpressions.Any())
            {
                where = " where " + string.Join($"{Environment.NewLine} and ", whereExpressions);
            }
            return where;
        }
        public PagedResult<DTO> ToResult()
        { 
            var sql = $@"{selectSql}
	                           {(useWhereRestricion?GetWhereRestriction():string.Empty)} 
	                           {(usePagingRestricion?GetPagingRestriction():string.Empty)}";
             
            var results = connection.
                Query<DTO>(sql, new { searchText, offset, limit = limit + 1 }).
                ToList();
            bool hasMore = false;
            if (results.Count > limit)
            {
                hasMore = true;
                //remove extra which is used to check if more results exists
                results.RemoveAt(results.Count - 1);
            }

            return new PagedResult<DTO> { Items = results.ToArray(), HasMore = hasMore };
        }
         
        public void Where(string expression)
        {
            whereExpressions.Add(expression);
        }

        
    }
    public static class PageResultQueryExtensions
    {
        public static PageResultQuery<DTO> PageQuery<DTO>(this IRepository repository, string orderby, int limit, int offset, bool desc)
        {
            return new PageResultQuery<DTO>(repository.GetConnection())
                .PageIs(orderby, limit, offset, desc);
        }
    }
}
