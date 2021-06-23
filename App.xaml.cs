using System;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.Foundation.Metadata;
using Windows.UI;
using Windows.UI.Popups;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace Perfect_OCR
{
    /// <summary>
    ///Fornece o comportamento específico do aplicativo para complementar a classe Application padrão.
    /// </summary>
    sealed partial class App : Application
    { 
        /// <summary>
        /// Inicializa o objeto singleton do aplicativo.  Esta é a primeira linha de código criado
        /// executado e, como tal, é o equivalente lógico de main() ou WinMain().
        /// </summary>
        public App()
        {
            this.InitializeComponent();
            this.Suspending += OnSuspending;
        }

        Frame rootFrame = Window.Current.Content as Frame;

        protected override void OnFileActivated(FileActivatedEventArgs args)
        {
            try
            {
                AtualizarBarra();
                base.OnFileActivated(args);
         
                if (rootFrame == null)
                {
                    rootFrame = new Frame();
                    rootFrame.Navigate(typeof(MainPage), args);
                }
                else
                {
                    rootFrame.Navigate(typeof(MainPage), args);

                }

                Window.Current.Content = rootFrame;
                Window.Current.Activate();
            }
            catch { }

        }


        /// <summary>
        /// Chamado quando o aplicativo é iniciado normalmente pelo usuário final.  Outros pontos de entrada
        /// serão usados, por exemplo, quando o aplicativo for iniciado para abrir um arquivo específico.
        /// </summary>
        /// <param name="e">Detalhes sobre a solicitação e o processo de inicialização.</param>
        protected async override void OnLaunched(LaunchActivatedEventArgs e)
        { 
            AtualizarBarra();

            // Não repita a inicialização do aplicativo quando a Janela já tiver conteúdo,
            // apenas verifique se a janela está ativa
            if (rootFrame == null)
            {
                // Crie um Quadro para atuar como o contexto de navegação e navegue para a primeira página
                rootFrame = new Frame();

                rootFrame.NavigationFailed += OnNavigationFailed;

                if (e.PreviousExecutionState == ApplicationExecutionState.Terminated)
                {
                    // TODO: Carregue o estado do aplicativo suspenso anteriormente
                }

                // Coloque o quadro na Janela atual
                Window.Current.Content = rootFrame;
            }

            if (e.PrelaunchActivated == false)
            {
                if (rootFrame.Content == null)
                {
                    // Quando a pilha de navegação não for restaurada, navegar para a primeira página,
                    // configurando a nova página passando as informações necessárias como um parâmetro
                    // parâmetro
                    try { rootFrame.Navigate(typeof(MainPage), e.Arguments); }catch(Exception X) { await new MessageDialog(X.Message).ShowAsync(); }
                } 
                Window.Current.Activate();
            }
        }

        private async void AtualizarBarra()
        { 
            if (ApiInformation.IsTypePresent("Windows.UI.ViewManagement.StatusBar"))
            {
                var statusbar = StatusBar.GetForCurrentView();
                statusbar.BackgroundColor = ((SolidColorBrush)App.Current.Resources["IsBackgroundToolBar"]).Color;
                statusbar.ForegroundColor = Colors.WhiteSmoke;
                statusbar.BackgroundOpacity = 100;
                await statusbar.HideAsync();
            }
            else
            {
                var view = ApplicationView.GetForCurrentView().TitleBar;
                view.BackgroundColor = ((SolidColorBrush)App.Current.Resources["IsBackgroundToolBar"]).Color;
                view.ForegroundColor = Colors.White;
                view.ButtonBackgroundColor = ((SolidColorBrush)App.Current.Resources["IsBackgroundToolBar"]).Color;
                view.ButtonForegroundColor = Colors.White;
            }  
        }
        
        /// <summary>
        /// Chamado quando ocorre uma falha na Navegação para uma determinada página
        /// </summary>
        /// <param name="sender">O Quadro com navegação com falha</param>
        /// <param name="e">Detalhes sobre a falha na navegação</param>
        void OnNavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            throw new Exception("Failed to load Page " + e.SourcePageType.FullName);
        }

        /// <summary>
        /// Chamado quando a execução do aplicativo está sendo suspensa.  O estado do aplicativo é salvo
        /// sem saber se o aplicativo será encerrado ou retomado com o conteúdo
        /// da memória ainda intacto.
        /// </summary>
        /// <param name="sender">A fonte da solicitação de suspensão.</param>
        /// <param name="e">Detalhes sobre a solicitação de suspensão.</param>
        private void OnSuspending(object sender, SuspendingEventArgs e)
        {
            var deferral = e.SuspendingOperation.GetDeferral();
            //TODO: Salvar o estado do aplicativo e parar qualquer atividade em segundo plano
            deferral.Complete();
        }
    }
}
