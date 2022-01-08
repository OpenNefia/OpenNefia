namespace OpenNefia.Core.UI
{
    public delegate void PageChangedDelegate(int newPage, int newPageCount);

    public interface IUiPaged
    {
        int CurrentPage { get; }
        int PageCount { get; }

        /// <summary>
        /// Changes the page of this element.
        /// </summary>
        /// <param name="page">New page. It should be clamped within [0, pageCount).</param>
        /// <returns>True if the new page is different than the old page.</returns>
        bool SetPage(int page);

        /// <summary>
        /// Decrements the page count.
        /// </summary>
        /// <returns>True if the new page is different than the old page.</returns>
        bool PageBackward();

        /// <summary>
        /// Increments the page count.
        /// </summary>
        /// <returns>True if the new page is different than the old page.</returns>
        bool PageForward();

        event PageChangedDelegate? OnPageChanged;
    }
}