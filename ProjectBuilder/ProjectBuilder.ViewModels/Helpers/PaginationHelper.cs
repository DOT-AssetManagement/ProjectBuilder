using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ProjectBuilder.ViewModels
{
    public class PaginationHelper<T>
    {
        private readonly List<T> _source;
        private Func<int, int,CancellationToken ,Task<List<T>>> _itemsProvider;
        int[]  _possibleValues = new int[] { 10, 20, 30, 40, 50, 60, 70, 80, 90, 100, 150, 200 };
        private int _itemsPerPage;
        private long _sourceCount;
        public int PagesCount { get; private set; }
        public long SourceCount 
        { 
            get { return _sourceCount; }
            set
            {
                if(_sourceCount != value)
                {
                    _sourceCount = value;
                    CalculateItemsPerPage(ItemsPerPage);
                }
            }
        }
        public int ItemsPerPage 
        {
            get { return _itemsPerPage; } 
            set
            {
                if(value != _itemsPerPage)
                {
                  _itemsPerPage = value;
                  CalculateItemsPerPage(value);
                }
            }
        } 
        public PaginationHelper(List<T> source)
        {
            _source = source;
            ItemsPerPage = 10;
            SourceCount = _source.Count;
        }
        public PaginationHelper(long sourceCount,Func<int,int,CancellationToken,Task<List<T>>> itemsProvider)
        {
            _itemsProvider = itemsProvider;
            ItemsPerPage = 10;
            SourceCount = sourceCount;
        }
        /// <summary>
        /// gets a page from a preloaded source
        /// </summary>
        /// <param name="selectedPageIndex">the index where the items will start</param>
        /// <returns></returns>
        public IEnumerable<T> CreatePage(int selectedPageIndex)
        {
            var index = selectedPageIndex * ItemsPerPage;
            if ((index + ItemsPerPage) < _source.Count)
                return _source.GetRange(index, ItemsPerPage);
            else if (index < _source.Count)
                return _source.GetRange(index, _source.Count - index);
            return _source;
        }
        /// <summary>
        /// gets the requested page according to the given index the page will be loaded upon request
        /// </summary>
        /// <param name="selectedPageIndex">the index where the items will start</param>
        /// <returns></returns>
        public async Task<List<T>> CreatePageAsync(int selectedPageIndex,CancellationToken token = default)
        {
            if (selectedPageIndex < 0)
                selectedPageIndex = 0;
            var index = selectedPageIndex * ItemsPerPage;
            if ((index + ItemsPerPage) < SourceCount)
                return await _itemsProvider(index, ItemsPerPage,token);
            else if (index < SourceCount)
                return await _itemsProvider(index, (int)(SourceCount - index),token);
            return await _itemsProvider(0, (int)(SourceCount - index),token);
        }
        private void CalculateItemsPerPage(int itemsPerPage)
        {
            PagesCount = 0;
            if (SourceCount > itemsPerPage)
            {
                // TODO- possible cast exception
                PagesCount = (int)(SourceCount / itemsPerPage);
                if (SourceCount % itemsPerPage > 0)
                    PagesCount += 1;
            }
            else PagesCount = 1;
        }
        public void CalculteItemsPerPage()
        {
          
            foreach (var value in _possibleValues)
            {
                var result = SourceCount / value;
                if (result / 10 < 1)
                    ItemsPerPage = 10;
               else if (result / 10 >= 2 && result <= 100)
                    ItemsPerPage = value;
                else if (result > 100)
                    ItemsPerPage = 200;
            }
        }
    }
}
