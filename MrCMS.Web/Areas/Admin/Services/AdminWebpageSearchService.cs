using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using MrCMS.Entities.Documents;
using MrCMS.Entities.Documents.Web;
using MrCMS.Entities.Indexes;
using MrCMS.Entities.Multisite;
using MrCMS.Helpers;
using MrCMS.Indexing.Querying;
using MrCMS.Paging;
using MrCMS.Services;
using MrCMS.Web.Areas.Admin.Models.Search;
using NHibernate;
using NHibernate.Criterion;

namespace MrCMS.Web.Areas.Admin.Services
{
    public class AdminWebpageSearchService : IAdminWebpageSearchService
    {
        private readonly ISearcher<Webpage, AdminWebpageIndexDefinition> _documentSearcher;
        private readonly IDocumentService _documentService;
        private readonly ISession _session;
        private readonly Site _site;

        public AdminWebpageSearchService(ISearcher<Webpage, AdminWebpageIndexDefinition> documentSearcher, IDocumentService documentService, ISession session, Site site)
        {
            _documentSearcher = documentSearcher;
            _documentService = documentService;
            _session = session;
            _site = site;
        }

        public IPagedList<Webpage> Search(AdminWebpageSearchQuery model)
        {
            return _documentSearcher.Search(model.GetQuery(), model.Page);
        }

        public IEnumerable<QuickSearchResults> QuickSearch(AdminWebpageSearchQuery model)
        {
            return _documentSearcher.Search(model.GetQuery(), model.Page, 10).Select(x => new QuickSearchResults
                                                                                          {
                                                                                              Id = x.Id,
                                                                                              Name = x.Name,
                                                                                              CreatedOn = x.CreatedOn.ToShortDateString().ToString(),
                                                                                              Type = x.GetType().Name.ToString()
                                                                                          });
        }

        public IEnumerable<Document> GetBreadCrumb(int? parentId)
        {
            return _documentService.GetParents(parentId).Reverse();
        }

        public List<SelectListItem> GetDocumentTypes(string type)
        {
            return DocumentMetadataHelper.DocumentMetadatas
                                   .BuildSelectItemList(definition => definition.Name, definition => definition.TypeName,
                                                        definition => definition.TypeName == type, "Select type");
        }

        public List<SelectListItem> GetParentsList()
        {
            var parentIds = _session.QueryOver<Webpage>()
            .Where(webpage => webpage.Parent != null)
                .SelectList(
                    builder =>
                        builder.Select(Projections.Distinct(Projections.Property<Webpage>(webpage => webpage.Parent.Id))))
                .Cacheable()
                .List<int>().ToList();
            var rootWebpages = _session.QueryOver<Webpage>()
                .Where(webpage => webpage.Parent == null && webpage.Site.Id == _site.Id && webpage.Id.IsIn(parentIds))
                .OrderBy(webpage => webpage.DisplayOrder)
                .Asc.List();
            var selectListItems =
                GetPageListItems(
                    rootWebpages, parentIds, 1).ToList();
            selectListItems.Insert(0, new SelectListItem { Selected = false, Text = "Root", Value = "0" });
            return selectListItems;
        }

        private IEnumerable<SelectListItem> GetPageListItems(IEnumerable<Webpage> pages, List<int> parentIds, int depth)
        {
            var items = new List<SelectListItem>();

            foreach (var node in pages.Where(node => parentIds.Contains(node.Id)))
            {
                items.Add(new SelectListItem { Selected = false, Text = GetDashes(depth) + node.Name, Value = node.Id.ToString() });
                items.AddRange(GetPageListItems(_session.QueryOver<Webpage>()
                    .Where(webpage => webpage.Parent.Id == node.Id && webpage.Id.IsIn(parentIds))
                    .OrderBy(webpage => webpage.DisplayOrder)
                    .Asc.Cacheable()
                    .List(), parentIds, depth + 1));
            }

            return items;
        }

        private string GetDashes(int depth)
        {
            return string.Empty.PadRight(depth * 2, '-');
        }

    }
}