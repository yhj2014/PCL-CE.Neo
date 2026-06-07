using System;
using System.Drawing;
using System.Windows.Forms;
using Microsoft.Extensions.DependencyInjection;
using PCL_CE.Neo.Core;
using PCL_CE.Neo.UI;
using PCL_CE.Neo.Platform.Windows;

namespace PCL_CE.Neo.App;

public class MainForm : Form
{
    private Button _navHomeButton;
    private Button _navLaunchButton;
    private Button _navVersionsButton;
    private Button _navInstancesButton;
    private Button _navLoginButton;
    private Button _navToolsButton;
    private Button _navSettingsButton;
    
    private Label _titleLabel;
    private Label _statusLabel;
    private Label _welcomeLabel;
    private Label _descriptionLabel;
    
    private Panel _sidebarPanel;
    private Panel _contentPanel;
    private Panel _headerPanel;
    
    private Button _themeLightButton;
    private Button _themeDarkButton;
    
    private bool _isDarkTheme = false;

    public MainForm()
    {
        InitializeComponents();
        InitializeServices();
        ApplyTheme();
    }

    private void InitializeComponents()
    {
        // Form settings
        Text = "PCL CE Neo";
        Size = new Size(1000, 700);
        MinimumSize = new Size(800, 500);
        StartPosition = FormStartPosition.CenterScreen;
        Font = new Font("Segoe UI", 9.75F, FontStyle.Regular, GraphicsUnit.Point);
        
        // Main layout
        _headerPanel = new Panel
        {
            Dock = DockStyle.Top,
            Height = 50,
            BackColor = Color.FromArgb(45, 45, 48)
        };
        
        _sidebarPanel = new Panel
        {
            Dock = DockStyle.Left,
            Width = 200,
            BackColor = Color.FromArgb(37, 37, 38)
        };
        
        _contentPanel = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = Color.FromArgb(32, 32, 32)
        };
        
        // Header content
        _titleLabel = new Label
        {
            Text = "PCL CE Neo",
            Font = new Font("Segoe UI", 14F, FontStyle.Bold),
            ForeColor = Color.White,
            Location = new Point(15, 10),
            AutoSize = true
        };
        
        _themeLightButton = new Button
        {
            Text = "☀️",
            Size = new Size(36, 36),
            Location = new Point(Width - 95, 7),
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.Transparent,
            ForeColor = Color.White,
            Cursor = Cursors.Hand
        };
        _themeLightButton.FlatAppearance.BorderSize = 0;
        _themeLightButton.Click += OnLightThemeClick;
        
        _themeDarkButton = new Button
        {
            Text = "🌙",
            Size = new Size(36, 36),
            Location = new Point(Width - 50, 7),
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.Transparent,
            ForeColor = Color.White,
            Cursor = Cursors.Hand
        };
        _themeDarkButton.FlatAppearance.BorderSize = 0;
        _themeDarkButton.Click += OnDarkThemeClick;
        
        _headerPanel.Controls.Add(_titleLabel);
        _headerPanel.Controls.Add(_themeLightButton);
        _headerPanel.Controls.Add(_themeDarkButton);
        
        // Sidebar navigation
        var yPos = 20;
        
        _navHomeButton = CreateNavButton("🏠  首页", yPos);
        _navHomeButton.Click += OnNavHomeClick;
        yPos += 45;
        
        _navLaunchButton = CreateNavButton("🎮  启动游戏", yPos);
        _navLaunchButton.Click += OnNavLaunchClick;
        yPos += 45;
        
        _navVersionsButton = CreateNavButton("📦  版本下载", yPos);
        _navVersionsButton.Click += OnNavVersionsClick;
        yPos += 45;
        
        _navInstancesButton = CreateNavButton("💾  实例管理", yPos);
        _navInstancesButton.Click += OnNavInstancesClick;
        yPos += 45;
        
        _navLoginButton = CreateNavButton("👤  账号登录", yPos);
        _navLoginButton.Click += OnNavLoginClick;
        yPos += 55;
        
        var separator = new Panel
        {
            Height = 1,
            Width = 180,
            Location = new Point(10, yPos),
            BackColor = Color.FromArgb(55, 55, 58)
        };
        _sidebarPanel.Controls.Add(separator);
        yPos += 20;
        
        _navToolsButton = CreateNavButton("🛠️  工具箱", yPos);
        _navToolsButton.Click += OnNavToolsClick;
        yPos += 45;
        
        _navSettingsButton = CreateNavButton("⚙️  设置", yPos);
        _navSettingsButton.Click += OnNavSettingsClick;
        
        _sidebarPanel.Controls.Add(_navHomeButton);
        _sidebarPanel.Controls.Add(_navLaunchButton);
        _sidebarPanel.Controls.Add(_navVersionsButton);
        _sidebarPanel.Controls.Add(_navInstancesButton);
        _sidebarPanel.Controls.Add(_navLoginButton);
        _sidebarPanel.Controls.Add(_navToolsButton);
        _sidebarPanel.Controls.Add(_navSettingsButton);
        
        // Content panel
        _welcomeLabel = new Label
        {
            Text = "欢迎使用 PCL CE Neo",
            Font = new Font("Segoe UI", 24F, FontStyle.Bold),
            ForeColor = Color.White,
            Location = new Point(40, 40),
            AutoSize = true
        };
        
        _descriptionLabel = new Label
        {
            Text = "跨平台 Minecraft 启动器\n当前版本：v0.0.1-alpha (开发版)",
            Font = new Font("Segoe UI", 11F),
            ForeColor = Color.FromArgb(200, 200, 200),
            Location = new Point(40, 95),
            AutoSize = true
        };
        
        _statusLabel = new Label
        {
            Text = "✅ 核心架构已就绪",
            Font = new Font("Segoe UI", 10F),
            ForeColor = Color.FromArgb(80, 200, 120),
            Location = new Point(40, 160),
            AutoSize = true
        };
        
        var featurePanel = new Panel
        {
            Location = new Point(40, 200),
            Size = new Size(500, 300),
            BackColor = Color.FromArgb(45, 45, 48),
            BorderStyle = BorderStyle.FixedSingle
        };
        
        var featureTitle = new Label
        {
            Text = "功能预览",
            Font = new Font("Segoe UI", 12F, FontStyle.Bold),
            ForeColor = Color.White,
            Location = new Point(15, 15),
            AutoSize = true
        };
        
        var features = new[]
        {
            "🎯 跨平台支持 (Windows/macOS/Linux)",
            "🎮 完整的游戏启动功能",
            "📦 游戏版本下载与管理",
            "💾 多实例管理",
            "🌐 多人联机支持",
            "🎨 深色/浅色主题切换"
        };
        
        var featureY = 50;
        foreach (var feature in features)
        {
            var featureLabel = new Label
            {
                Text = feature,
                Font = new Font("Segoe UI", 10F),
                ForeColor = Color.FromArgb(220, 220, 220),
                Location = new Point(20, featureY),
                AutoSize = true
            };
            featurePanel.Controls.Add(featureLabel);
            featureY += 35;
        }
        
        featurePanel.Controls.Add(featureTitle);
        _contentPanel.Controls.Add(_welcomeLabel);
        _contentPanel.Controls.Add(_descriptionLabel);
        _contentPanel.Controls.Add(_statusLabel);
        _contentPanel.Controls.Add(featurePanel);
        
        // Add all to form
        Controls.Add(_contentPanel);
        Controls.Add(_sidebarPanel);
        Controls.Add(_headerPanel);
        
        // Handle resize
        Resize += OnFormResize;
    }

    private Button CreateNavButton(string text, int y)
    {
        var button = new Button
        {
            Text = text,
            Size = new Size(180, 40),
            Location = new Point(10, y),
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.Transparent,
            ForeColor = Color.White,
            TextAlign = ContentAlignment.MiddleLeft,
            Padding = new Padding(15, 0, 0, 0),
            Cursor = Cursors.Hand
        };
        button.FlatAppearance.BorderSize = 0;
        button.FlatAppearance.MouseOverBackColor = Color.FromArgb(55, 55, 58);
        button.FlatAppearance.MouseDownBackColor = Color.FromArgb(65, 65, 68);
        return button;
    }

    private void InitializeServices()
    {
        try
        {
            var services = new ServiceCollection();
            services.AddCoreServices();
            services.AddUIServices();
            services.AddWindowsPlatformServices();
            
            var serviceProvider = services.BuildServiceProvider();
            _statusLabel.Text = "✅ 核心服务已初始化";
        }
        catch (Exception ex)
        {
            _statusLabel.Text = "❌ 服务初始化失败";
            _statusLabel.ForeColor = Color.FromArgb(220, 80, 80);
            MessageBox.Show($"服务初始化失败：{ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void ApplyTheme()
    {
        if (_isDarkTheme)
        {
            _headerPanel.BackColor = Color.FromArgb(45, 45, 48);
            _sidebarPanel.BackColor = Color.FromArgb(37, 37, 38);
            _contentPanel.BackColor = Color.FromArgb(32, 32, 32);
            _titleLabel.ForeColor = Color.White;
            _welcomeLabel.ForeColor = Color.White;
            _descriptionLabel.ForeColor = Color.FromArgb(200, 200, 200);
            
            foreach (Control control in _sidebarPanel.Controls)
            {
                if (control is Button btn)
                {
                    btn.ForeColor = Color.White;
                    btn.FlatAppearance.MouseOverBackColor = Color.FromArgb(55, 55, 58);
                }
            }
        }
        else
        {
            _headerPanel.BackColor = Color.FromArgb(250, 250, 250);
            _sidebarPanel.BackColor = Color.FromArgb(240, 240, 240);
            _contentPanel.BackColor = Color.FromArgb(245, 245, 245);
            _titleLabel.ForeColor = Color.FromArgb(30, 30, 30);
            _welcomeLabel.ForeColor = Color.FromArgb(30, 30, 30);
            _descriptionLabel.ForeColor = Color.FromArgb(80, 80, 80);
            
            foreach (Control control in _sidebarPanel.Controls)
            {
                if (control is Button btn)
                {
                    btn.ForeColor = Color.FromArgb(30, 30, 30);
                    btn.FlatAppearance.MouseOverBackColor = Color.FromArgb(220, 220, 220);
                }
            }
        }
    }

    private void OnFormResize(object? sender, EventArgs e)
    {
        _themeLightButton.Location = new Point(Width - 95, 7);
        _themeDarkButton.Location = new Point(Width - 50, 7);
    }

    private void OnLightThemeClick(object? sender, EventArgs e)
    {
        _isDarkTheme = false;
        ApplyTheme();
    }

    private void OnDarkThemeClick(object? sender, EventArgs e)
    {
        _isDarkTheme = true;
        ApplyTheme();
    }

    private void OnNavHomeClick(object? sender, EventArgs e)
    {
        ShowPageMessage("首页");
    }

    private void OnNavLaunchClick(object? sender, EventArgs e)
    {
        ShowPageMessage("启动游戏");
    }

    private void OnNavVersionsClick(object? sender, EventArgs e)
    {
        ShowPageMessage("版本下载");
    }

    private void OnNavInstancesClick(object? sender, EventArgs e)
    {
        ShowPageMessage("实例管理");
    }

    private void OnNavLoginClick(object? sender, EventArgs e)
    {
        ShowPageMessage("账号登录");
    }

    private void OnNavToolsClick(object? sender, EventArgs e)
    {
        ShowPageMessage("工具箱");
    }

    private void OnNavSettingsClick(object? sender, EventArgs e)
    {
        ShowPageMessage("设置");
    }

    private void ShowPageMessage(string pageName)
    {
        _statusLabel.Text = $"📍 已导航到：{pageName}";
        _statusLabel.ForeColor = Color.FromArgb(80, 150, 220);
    }
}
