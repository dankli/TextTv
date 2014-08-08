namespace TextTv.Shared.Infrastructure
{
    public class PageNumberHandler
    {
        private int _currentPage;
        private int _lastPage;

        public PageNumberHandler(int currentPage)
        {
            _currentPage = currentPage;
            _lastPage = currentPage;
        }

        public int CurrentPage
        {
            get { return this._currentPage; }
        }

        public void SetCurrentPage(int page)
        {
            _lastPage = page;
            _currentPage = page;
        }

        public void Next()
        {
            _lastPage = _currentPage;
            _currentPage++;
        }

        public void Back()
        {
            _lastPage = _currentPage;
            _currentPage--;
            if (_currentPage < 100)
            {
                _currentPage = 100;
            }
        }

        public void Continue()
        {
            if (_lastPage > _currentPage)
            {
                this.Back();
            }
            else
            {
                this.Next();
            }

            if (_currentPage < 100 || _currentPage >= 1000)
            {
                _currentPage = 100;
                _lastPage = 100;
            }
        }
    }
}
