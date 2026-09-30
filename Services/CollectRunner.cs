using System.Diagnostics;
using FileInfoViewer.Models;

namespace FileInfoViewer.Services;

/// <summary>
/// Runs <see cref="FileInfoCollector.Collect"/> on a background thread so the window stays responsive.
/// If hashing takes noticeably long, a progress window with a Cancel button is shown.
/// </summary>
internal static class CollectRunner
{
    // Files that hash faster than this never show the progress window
    private const int ShowDialogAfterMs = 500;

    /// <param name="owner">The window to centre the progress window on and to disable while working (null: none).</param>
    public static async Task<FileInfoModel> RunAsync(string filePath, Form? owner)
    {
        using var cts = new CancellationTokenSource();
        var clock = Stopwatch.StartNew();
        HashProgressForm? dialog = null;
        var finished = false;

        // Progress<T> raises its event on the UI thread (the context this method was called from)
        var progress = new Progress<(long Done, long Total)>(p =>
        {
            if (finished) return;
            if (dialog == null)
            {
                if (clock.ElapsedMilliseconds < ShowDialogAfterMs) return;
                dialog = new HashProgressForm(Path.GetFileName(filePath));
                dialog.CancelRequested += (_, _) => cts.Cancel();
                if (owner != null) dialog.Show(owner); else dialog.Show();
            }
            dialog.SetProgress(p.Done, p.Total);
        });

        if (owner != null) owner.Enabled = false;
        try
        {
            return await Task.Run(() => FileInfoCollector.Collect(filePath, progress, cts.Token));
        }
        finally
        {
            finished = true;
            if (owner != null) owner.Enabled = true;
            dialog?.Finish();
            dialog?.Dispose();
        }
    }
}
