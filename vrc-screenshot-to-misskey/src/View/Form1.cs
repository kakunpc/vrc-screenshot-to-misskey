using System.ComponentModel;
using vrc_screenshot_to_misskey.ApplicationService;
using vrc_screenshot_to_misskey.Domain;

namespace vrc_screenshot_to_misskey;

public partial class Form1 : Form
{
    private NotifyIcon notifyIcon;

    private MisskeyAutoUploadService _misskeyAutoUploadService;

    public Form1(MisskeyAutoUploadService misskeyAutoUploadService)
    {
        _misskeyAutoUploadService = misskeyAutoUploadService;

        // タスクバーに表示しない
        ShowInTaskbar = false;
        SetComponents();
        Application.ApplicationExit += ApplicationOnApplicationExit;

        // 処理を実行させる
        _ = _misskeyAutoUploadService.RunAsync();
    }

    private void ApplicationOnApplicationExit(object? sender, EventArgs e)
    {
        // アイコンを消す
        notifyIcon.Dispose();
    }

    private void SetComponents()
    {
        var resources = new ComponentResourceManager(typeof(Form1));

        notifyIcon = new NotifyIcon();

        // アイコンの設定
        notifyIcon.Icon = (Icon)resources.GetObject("$this.Icon")!;
        // アイコンを表示する
        notifyIcon.Visible = true;
        // アイコンにマウスポインタを合わせたときのテキスト
        notifyIcon.Text = "vrc-screenshot-to-misskey";

        // コンテキストメニュー
        var contextMenuStrip = new ContextMenuStrip();
        var toolStripMenuItem = new ToolStripMenuItem();
        toolStripMenuItem.Text = "&終了";
        toolStripMenuItem.Click += ToolStripMenuItem_Click;
        contextMenuStrip.Items.Add(toolStripMenuItem);
        notifyIcon.ContextMenuStrip = contextMenuStrip;
    }

    private void ToolStripMenuItem_Click(object? sender, EventArgs e)
    {
        // 終了処理
        _ = _misskeyAutoUploadService.Stop(
            () =>
            {
                Application.Exit();
            });
    }
}
