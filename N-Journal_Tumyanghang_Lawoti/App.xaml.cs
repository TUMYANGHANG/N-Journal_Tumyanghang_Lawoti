namespace N_Journal_Tumyanghang_Lawoti;

public partial class App : Application
{
	public App()
	{
		InitializeComponent();
	}

	protected override Window CreateWindow(IActivationState? activationState)
	{
		return new Window(new MainPage()) { Title = "N-Journal_Tumyanghang_Lawoti" };
	}
}
