using System.Globalization;
using System.Windows.Forms;

namespace HusqvarnaAutomowerConnect.App.Views;

internal sealed class SettingsEditDialog
{
    public SettingsEditDialog(
        string applicationKey,
        string redirectUri,
        int refreshIntervalSeconds,
        string minimumLogLevel)
    {
        ApplicationKey = applicationKey;
        RedirectUri = redirectUri;
        RefreshIntervalSeconds = refreshIntervalSeconds;
        MinimumLogLevel = minimumLogLevel;
    }

    public string ApplicationKey { get; private set; }

    public string RedirectUri { get; private set; }

    public int RefreshIntervalSeconds { get; private set; }

    public string MinimumLogLevel { get; private set; }

    public string? ApplicationSecret { get; private set; }

    public bool ShowDialog()
    {
        using Form form = new()
        {
            Text = "Paramètres Husqvarna Automower Connect",
            StartPosition = FormStartPosition.CenterScreen,
            FormBorderStyle = FormBorderStyle.FixedDialog,
            MaximizeBox = false,
            MinimizeBox = false,
            Width = 620,
            Height = 420
        };

        TableLayoutPanel layout = new()
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 6,
            Padding = new Padding(16),
            AutoSize = true
        };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 220));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

        TextBox applicationKeyBox = CreateTextBox(ApplicationKey);
        TextBox redirectUriBox = CreateTextBox(RedirectUri);
        TextBox refreshIntervalBox = CreateTextBox(RefreshIntervalSeconds.ToString(CultureInfo.InvariantCulture));
        TextBox minimumLogLevelBox = CreateTextBox(MinimumLogLevel);
        TextBox applicationSecretBox = CreateTextBox(string.Empty, usePasswordChar: true);

        AddRow(layout, "Clé d'application", applicationKeyBox, 0);
        AddRow(layout, "URI de redirection", redirectUriBox, 1);
        AddRow(layout, "Intervalle de rafraîchissement (secondes)", refreshIntervalBox, 2);
        AddRow(layout, "Niveau de log", minimumLogLevelBox, 3);
        AddRow(layout, "Secret d'application", applicationSecretBox, 4);

        Label hint = new()
        {
            Text = "Laissez le secret vide pour conserver le secret local actuel.",
            Dock = DockStyle.Fill,
            AutoSize = true
        };
        layout.Controls.Add(hint, 0, 5);
        layout.SetColumnSpan(hint, 2);

        FlowLayoutPanel buttons = new()
        {
            Dock = DockStyle.Bottom,
            FlowDirection = FlowDirection.RightToLeft,
            Padding = new Padding(16),
            Height = 56
        };

        Button okButton = new()
        {
            Text = "Enregistrer",
            DialogResult = DialogResult.OK
        };
        okButton.Click += (_, _) =>
        {
            if (!TryApply(applicationKeyBox.Text, redirectUriBox.Text, refreshIntervalBox.Text, minimumLogLevelBox.Text, applicationSecretBox.Text, out string? errorMessage))
            {
                MessageBox.Show(form, errorMessage, "Paramètres invalides", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            form.DialogResult = DialogResult.OK;
            form.Close();
        };

        Button cancelButton = new()
        {
            Text = "Annuler",
            DialogResult = DialogResult.Cancel
        };

        buttons.Controls.Add(okButton);
        buttons.Controls.Add(cancelButton);

        form.AcceptButton = okButton;
        form.CancelButton = cancelButton;
        form.Controls.Add(layout);
        form.Controls.Add(buttons);

        DialogResult result = form.ShowDialog();
        return result == DialogResult.OK;
    }

    private bool TryApply(
        string applicationKey,
        string redirectUri,
        string refreshIntervalText,
        string minimumLogLevel,
        string applicationSecret,
        out string? errorMessage)
    {
        applicationKey = applicationKey.Trim();
        redirectUri = redirectUri.Trim();
        minimumLogLevel = minimumLogLevel.Trim();
        refreshIntervalText = refreshIntervalText.Trim();
        applicationSecret = applicationSecret.Trim();

        if (string.IsNullOrWhiteSpace(applicationKey))
        {
            errorMessage = "La clé d'application est requise.";
            return false;
        }

        if (!Uri.TryCreate(redirectUri, UriKind.Absolute, out _))
        {
            errorMessage = "L'URI de redirection doit être une URI absolue valide.";
            return false;
        }

        if (!int.TryParse(refreshIntervalText, NumberStyles.Integer, CultureInfo.InvariantCulture, out int refreshIntervalSeconds))
        {
            errorMessage = "L'intervalle de rafraîchissement doit être un nombre entier.";
            return false;
        }

        if (refreshIntervalSeconds < 30)
        {
            errorMessage = "L'intervalle de rafraîchissement doit être au minimum de 30 secondes.";
            return false;
        }

        if (string.IsNullOrWhiteSpace(minimumLogLevel))
        {
            errorMessage = "Le niveau de log ne peut pas être vide.";
            return false;
        }

        ApplicationKey = applicationKey;
        RedirectUri = redirectUri;
        RefreshIntervalSeconds = refreshIntervalSeconds;
        MinimumLogLevel = minimumLogLevel;
        ApplicationSecret = string.IsNullOrWhiteSpace(applicationSecret) ? null : applicationSecret;
        errorMessage = null;
        return true;
    }

    private static TextBox CreateTextBox(string value, bool usePasswordChar = false) =>
        new()
        {
            Dock = DockStyle.Fill,
            Text = value,
            UseSystemPasswordChar = usePasswordChar
        };

    private static void AddRow(TableLayoutPanel layout, string label, Control control, int rowIndex)
    {
        Label textLabel = new()
        {
            Text = label,
            Dock = DockStyle.Fill,
            AutoSize = true
        };

        layout.Controls.Add(textLabel, 0, rowIndex);
        layout.Controls.Add(control, 1, rowIndex);
    }
}
