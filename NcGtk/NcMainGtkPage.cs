namespace NcGtk;

public sealed class NcMainGtkPage
{
    #region Public and private fields, properties, constructor

    private readonly Window _mainWindow;
    private readonly Builder _builder;
    private readonly Entry _entryIpAddress;
    private readonly ToggleButton _toggleServerMode;
    private readonly ToggleButton _toggleClientMode;
    private readonly Button _btnServerConnect;
    private readonly Button _btnServerDisconnect;
    private readonly Button _btnClientConnect;
    private readonly Button _btnClientDisconnect;
    private readonly TextView _textViewMessages;
    private readonly Entry _entryMessageInput;
    private readonly Button _btnSendMessage;
    private readonly Button _btnClearChat;
    private readonly NcMainViewModel _viewModel;

    public NcMainGtkPage()
    {
        var exePath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        var gladeFilePath = Path.Combine(exePath ?? ".", "NcMainWindow.glade");
        if (!File.Exists(gladeFilePath))
            throw new FileNotFoundException("Glade file not found!", gladeFilePath);

        _builder = new Builder();
        _builder.AddFromFile(gladeFilePath);

        _mainWindow = (Window)_builder.GetObject("mainWindow");
        if (_mainWindow is null)
            throw new Exception("Main window not found in Glade file!");
        _mainWindow.Destroyed += (o, e) => Application.Quit();
        _mainWindow.ShowAll();

        // Получить виджеты по id
        _entryIpAddress = (Entry)_builder.GetObject("entryIpAddress");
        _toggleServerMode = (ToggleButton)_builder.GetObject("toggleServerMode");
        _toggleClientMode = (ToggleButton)_builder.GetObject("toggleClientMode");
        _btnServerConnect = (Button)_builder.GetObject("btnServerConnect");
        _btnServerDisconnect = (Button)_builder.GetObject("btnServerDisconnect");
        _btnClientConnect = (Button)_builder.GetObject("btnClientConnect");
        _btnClientDisconnect = (Button)_builder.GetObject("btnClientDisconnect");
        _textViewMessages = (TextView)_builder.GetObject("textViewMessages");
        _entryMessageInput = (Entry)_builder.GetObject("entryMessageInput");
        _btnSendMessage = (Button)_builder.GetObject("btnSendMessage");
        _btnClearChat = (Button)_builder.GetObject("btnClearChat");

        // События
        _btnServerConnect.Clicked += OnServerConnectClicked;
        _btnServerDisconnect.Clicked += OnServerDisconnectClicked;
        _btnClientConnect.Clicked += OnClientConnectClicked;
        _btnClientDisconnect.Clicked += OnClientDisconnectClicked;
        _btnSendMessage.Clicked += OnSendMessageClicked;
        _btnClearChat.Clicked += OnClearChatClicked;
        _toggleServerMode.Toggled += OnToggleServerModeToggled;
        _toggleClientMode.Toggled += OnToggleClientModeToggled;

        // ViewModel
        _viewModel = new NcMainViewModel();

        // UI
        _entryIpAddress.Text = _viewModel.IpAddressPort;
        _toggleServerMode.Active = _viewModel.IsServerControlsVisible;
        _toggleClientMode.Active = _viewModel.IsClientControlsVisible;

        UpdateUIState();

        // Реакции ViewModel
        _viewModel.Messages.CollectionChanged += (s, e) =>
        {
            Application.Invoke(delegate
            {
                AppendMessagesToTextView();
            });
        };
    }

    #endregion

    #region Public and private methods

    public void Show() => _mainWindow.Show();

    /// <summary> Обновление текстового поля с сообщениями </summary>
    private void AppendMessagesToTextView()
    {
        var buffer = _textViewMessages.Buffer;
        buffer.Text = string.Join(Environment.NewLine, _viewModel.Messages);
        _textViewMessages.Buffer = buffer;
    }

    /// <summary> Обновление состояния кнопок в зависимости от подключения </summary>
    private void UpdateUIState()
    {
        // Обновление видимости кнопок в зависимости от состояния подключения Сервера и Клиента
        _btnServerConnect.Sensitive = _viewModel.IsServerControlsVisible;
        _btnServerDisconnect.Sensitive = _viewModel.IsServerControlsVisible;
        _btnClientConnect.Sensitive = _viewModel.IsClientControlsVisible;
        _btnClientDisconnect.Sensitive = _viewModel.IsClientControlsVisible;
    }

    private async void OnServerConnectClicked(object? sender, EventArgs e)
    {
        _viewModel.IpAddressPort = _entryIpAddress.Text;
        await _viewModel.ServerConnectCommand.ExecuteAsync(null);
        UpdateUIState();
    }

    private void OnServerDisconnectClicked(object? sender, EventArgs e)
    {
        _viewModel.ServerDisconnectCommand.Execute(null);
        UpdateUIState();
    }

    private async void OnClientConnectClicked(object? sender, EventArgs e)
    {
        _viewModel.IpAddressPort = _entryIpAddress.Text;
        await _viewModel.ClientConnectCommand.ExecuteAsync(null);
        UpdateUIState();
    }

    private void OnClientDisconnectClicked(object? sender, EventArgs e)
    {
        _viewModel.ClientDisconnectCommand.Execute(null);
        UpdateUIState();
    }

    private async void OnSendMessageClicked(object? sender, EventArgs e)
    {
        _viewModel.InputMessage = _entryMessageInput.Text;
        await _viewModel.SendMessageCommand.ExecuteAsync(null);
        _entryMessageInput.Text = string.Empty;
        // Задать фокус в поле ввода сообщения
        _entryMessageInput.GrabFocus();
        UpdateUIState();
    }

    private async void OnClearChatClicked(object? sender, EventArgs e)
    {
        await _viewModel.ClearCommand.ExecuteAsync(null);
        UpdateUIState();
    }

    private void OnToggleServerModeToggled(object? sender, EventArgs e)
    {
        if (sender is not ToggleButton toggleButton) return;
        if (toggleButton.Active)
        {
            _toggleClientMode.Active = false;
            _viewModel.IsServerControlsVisible = true;
            _viewModel.IsClientControlsVisible = false;
        }
        else if (!_toggleClientMode.Active)
        {
            // If the server is down and the client is not active, enable the client.
            _toggleClientMode.Active = true;
            _viewModel.IsClientControlsVisible = true;
            _viewModel.IsServerControlsVisible = false;
        }
        UpdateUIState();
    }

    private void OnToggleClientModeToggled(object? sender, EventArgs e)
    {
        if (sender is not ToggleButton toggleButton) return;
        if (toggleButton.Active)
        {
            _toggleServerMode.Active = false;
            _viewModel.IsClientControlsVisible = true;
            _viewModel.IsServerControlsVisible = false;
        }
        else if (!_toggleServerMode.Active)
        {
            _toggleServerMode.Active = true;
            _viewModel.IsServerControlsVisible = true;
            _viewModel.IsClientControlsVisible = false;
        }
        UpdateUIState();
    }

    #endregion
}
