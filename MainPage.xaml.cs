using System;
using System.Linq;
using Windows.Graphics.Imaging;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using System.Collections.Generic;
using Windows.ApplicationModel.Contacts;
using Windows.ApplicationModel.DataTransfer;
using Windows.ApplicationModel.Email;
using Windows.ApplicationModel.Resources.Core;
using Windows.Media.Ocr;
using Windows.Media.SpeechSynthesis;
using Windows.Storage.Pickers;
using Windows.UI.Notifications;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Media.Imaging;
using Microsoft.Toolkit.Uwp.UI.Animations;
using Windows.UI.Popups;
using Windows.Media.Capture;
using Windows.Foundation;
using Windows.Data.Xml.Dom;
using Windows.ApplicationModel.Resources;
using Windows.Foundation.Metadata;
using Perfect_OCR.BandoDeDados.ModelosDoBandoDeDados;
using Perfect_OCR.Modelos;
using Type = Perfect_OCR.Modelos.Type;
using Perfect_OCR.BandoDeDados.ConstructorDoBandoDeDados;

namespace Perfect_OCR
{
    public sealed partial class MainPage : Page
    {

        GuardadosController GuardadosC = new GuardadosController(); private SettingServices Model = new SettingServices();
        string AppTitle, itensList, txtResultDialogo, items, shared, unic, nameFotoNoNull, Ler_txt, NomeCriado;
        ResourceLoader loader = new ResourceLoader();
        private ResourceMap speechResourceMap;
        long ItemsLista => GuardadosC.Lista().Count;
        SoftwareBitmap bitmap;
        StorageFile foto;
        StorageFile file;
        bool isFalante, isText = true, isExitScan;
        private SpeechSynthesizer synthesizer;
        private ResourceContext speechContext;
        private static EmailMessage objEmail = new EmailMessage();
        Visibility v = Visibility.Visible; Visibility c = Visibility.Collapsed;
        public MainPage()
        {
            this.InitializeComponent();
            CarregarFontes();
            AppTitle = Model.DisplayName;
            NomeCriado = loader.GetString("NomeCriado");
            AutoInit();
        }


        private async void DialogoMessage(string message, string title)
        {
            var dialogo = new ContentDialog()
            {
                Content = message,
                BorderBrush = ((SolidColorBrush)App.Current.Resources["IsBorderBrush"]),
                Title = title,
                RequestedTheme = ElementTheme.Dark,
                Background = ((SolidColorBrush)App.Current.Resources["IsBackgroundForSplitViewAndFlyout"]),
                CloseButtonText = loader.GetString("Fechar")
            };
            await dialogo.ShowAsync();
        }

        private void CarregarFontes()
        {
            var fonts = Microsoft.Graphics.Canvas.Text.CanvasTextFormat.GetSystemFontFamilies();
            foreach (string f in fonts)
            {
                MenuFlyoutItem itemF = new MenuFlyoutItem
                {
                    Text = f,
                    FontFamily = new FontFamily(f)
                };
                itemF.Click += (Fonte, EventArgs) =>
                {
                    string fontF = ((MenuFlyoutItem)Fonte).Text;

                    TextResult.FontFamily = new FontFamily(fontF);

                    SettingServices.PutFontFamily(fontF);
                };
                //this.FontFaceMenuFlyout.Items.Clear();
                this.FontFaceMenuFlyout.Items.Add(itemF);
            }

        }

        private void Dialogo(Visibility a1, Visibility a2, Visibility a3, string txt)
        {
            TextResultD.Text = txt;
            isP.Visibility = a1;
            RootText.Visibility = a2;
            image.Visibility = a3;
            RootButtons.Visibility = a2;
            if (isP.Visibility == c) ButtonEnable(true); else ButtonEnable(false);
            if (RootText.Visibility == v) btnScanner.Content = loader.GetString("lerImagem2");
            else btnScanner.Content = loader.GetString("lerImagem");
        }

        private void ButtonEnable(bool e)
        {
            CameraC.IsEnabled = e;
            BrowserFoto.IsEnabled = e;
            btnScanner.IsEnabled = e;
        }

        private void InitializedFrostedGlass(UIElement uIElement, Canvas canvas, double ValueBlur)
        {
            if (canvas != null)
                canvas.Blur(ValueBlur);

            Windows.UI.Composition.Visual hostVisual = Windows.UI.Xaml.Hosting.ElementCompositionPreview.GetElementVisual(uIElement);
            Windows.UI.Composition.Compositor compositor = hostVisual.Compositor;

            // Cria um efeito de vidro com o pacote NuGet "Win2D.UWP"
            var glassEffect = new Microsoft.Graphics.Canvas.Effects.GaussianBlurEffect
            {
                BlurAmount = 1.9f,
                BorderMode = Microsoft.Graphics.Canvas.Effects.EffectBorderMode.Hard,
                Source = new Microsoft.Graphics.Canvas.Effects.ArithmeticCompositeEffect
                {
                    MultiplyAmount = 0,
                    Source1Amount = 0.5f,
                    Source2Amount = 0.5f,
                    Source1 = new Windows.UI.Composition.CompositionEffectSourceParameter("backdropBrush"),
                    Source2 = new Microsoft.Graphics.Canvas.Effects.ColorSourceEffect
                    {
                        Color = ((SolidColorBrush)App.Current.Resources["IsBackgroundToolBar"]).Color
                    }
                }
            };

            //  Create an instance of the effect and set its source to a CompositionBackdropBrush
            var effectFactory = compositor.CreateEffectFactory(glassEffect);
            var backdropBrush = compositor.CreateColorBrush();
            var effectBrush = effectFactory.CreateBrush();

            effectBrush.SetSourceParameter("backdropBrush", backdropBrush);

            // Create a Visual to contain the frosted glass effect
            var glassVisual = compositor.CreateSpriteVisual();
            glassVisual.Brush = effectBrush;

            // Add the blur as a child of the host in the visual tree
            Windows.UI.Xaml.Hosting.ElementCompositionPreview.SetElementChildVisual(uIElement, glassVisual);

            // Make sure size of glass host and glass visual always stay in sync
            var bindSizeAnimation = compositor.CreateExpressionAnimation("hostVisual.Size");
            bindSizeAnimation.SetReferenceParameter("hostVisual", hostVisual);

            glassVisual.StartAnimation("Size", bindSizeAnimation);
        }

        private void AutoInit()
        {
            synthesizer = new SpeechSynthesizer();
            speechContext = ResourceContext.GetForCurrentView();
            speechContext.Languages = new string[] { SpeechSynthesizer.DefaultVoice.Language };
            speechResourceMap = ResourceManager.Current.MainResourceMap.GetSubtree("LocalizationTTSResources");
            InitializeListboxVoiceChooser();
        }

        private void InitializeListboxVoiceChooser()
        {

            var voices = SpeechSynthesizer.AllVoices;

            foreach (VoiceInformation voice in voices.OrderBy(p => p.Language))
            {
                ComboBoxItem item = new ComboBoxItem
                {
                    Name = voice.DisplayName,
                    Tag = voice,
                    Content = $"{voice.DisplayName} - {loader.GetString("Language")}: {voice.Language}"
                };
                VoiceReader.Items.Add(item);

                if (Model.IsLanguageSelected == voice.Id)
                {
                    item.IsSelected = true;
                    VoiceReader.SelectedItem = item;
                }
            }
        }

        private void ListboxVoiceChooser_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBoxItem item = (ComboBoxItem)(VoiceReader.SelectedItem);
            VoiceInformation voice = (VoiceInformation)(item.Tag);
            synthesizer.Voice = voice;
            speechContext.Languages = new string[] { voice.Language };
            SettingServices.LanguageSelected(voice.Id);
        }

        private void IniciarResultadoDoScanner(string txt)
        {
            try
            {
                TextResult.Text = txt;
                Ler_txt = txt;
                RootButtons.Visibility = Visibility.Visible;
                Dialogo(c, v, c, "");
                isExitScan = false;
                isText = true;
            }
            catch (Exception X) { DialogoMessage(X.Message, ""); }
        }

        private void ListaUpdate()
        {
            Atualizando.Visibility = Visibility.Visible;
            _updadeList.Text = loader.GetString("updadeList");
            if (ItemsLista == 0)
            {
                btnDeleteHistorico.IsEnabled = false;
                Atualizando.Visibility = Visibility.Collapsed;

                listavazia.Visibility = Visibility.Visible;
            }
            else
            {
                btnDeleteHistorico.IsEnabled = true;
                listavazia.Visibility = Visibility.Collapsed;
                if (ItemsLista == 1) itensList = loader.GetString("itenList"); else itensList = loader.GetString("itensList");
                _updadeList.Text = $"{loader.GetString("addList")} {ItemsLista} {itensList}";
            }
            Lista.ItemsSource = GuardadosC.Lista().OrderByDescending(x => x.DataFoto);
            Atualizando.Visibility = Visibility.Collapsed;
        }

        private async void CameraC_Click(object sender, RoutedEventArgs e)
        {
            CameraCaptureUI captureUI = new CameraCaptureUI();
            captureUI.PhotoSettings.Format = CameraCaptureUIPhotoFormat.Jpeg;
            captureUI.PhotoSettings.CroppedSizeInPixels = new Size(200, 200);
            var fotoC = await captureUI.CaptureFileAsync(CameraCaptureUIMode.Photo);
            if (fotoC != null)
            {
                txtResultDialogo = loader.GetString("preparando") + Environment.NewLine + loader.GetString("Aguarde");
                if (isText)
                    Dialogo(v, v, c, txtResultDialogo);
                else
                    Dialogo(v, c, v, txtResultDialogo);
                var pastaDestino = await ApplicationData.Current.TemporaryFolder.CreateFolderAsync(loader.GetString("fotosFolder"), CreationCollisionOption.OpenIfExists);
                await fotoC.CopyAsync(pastaDestino, $"{loader.GetString("fotos")}.jpg", NameCollisionOption.ReplaceExisting);
                await fotoC.DeleteAsync();
                foto = await pastaDestino.GetFileAsync($"{loader.GetString("fotos")}.jpg");

                FotoVisible();

            }
            else { isP.Visibility = c; ButtonEnable(true); }

        }

        private async void FotoVisible()
        {
            var _foto = await foto.Properties.GetImagePropertiesAsync();
            var fotoDimensoes = _foto.Width;
            var stream = await foto.OpenAsync(FileAccessMode.Read);
            BitmapDecoder decoder = await BitmapDecoder.CreateAsync(stream);
            SoftwareBitmap software = await decoder.GetSoftwareBitmapAsync();
            SoftwareBitmap bitmap = SoftwareBitmap.Convert(software, BitmapPixelFormat.Bgra8, BitmapAlphaMode.Premultiplied);
            SoftwareBitmapSource bitmapSource = new SoftwareBitmapSource();
            await bitmapSource.SetBitmapAsync(bitmap);
            image.Source = bitmapSource;
            //BitmapImage i = new BitmapImage();
            //i.SetSource(stream);
            //image.Source = i;
            image.Width = fotoDimensoes;
            if (image.Width > RootElement.ActualWidth) image.Width = RootElement.ActualWidth - 10;
            if (image.Height > RootElement.ActualHeight) image.Height = RootElement.ActualHeight - 10;
            image.Margin = new Thickness(2, 1, 2, 1);
            ButtonScannerEnabled();

            Dialogo(c, c, v, txtResultDialogo);
            isExitScan = true;
            isText = false;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            DataTransferManager dataTransferManager = DataTransferManager.GetForCurrentView();
            dataTransferManager.DataRequested += DataTransferManager_DataRequested;
            if (e.Parameter is Windows.ApplicationModel.Activation.IActivatedEventArgs args)
            {
                if (args.Kind == Windows.ApplicationModel.Activation.ActivationKind.File)
                {
                    try
                    {
                        string sxt = (args as Windows.ApplicationModel.Activation.FileActivatedEventArgs).Files[0].Path;
                        foto = (StorageFile)(args as Windows.ApplicationModel.Activation.FileActivatedEventArgs).Files[0];
                        txtResultDialogo = loader.GetString("preparando") + Environment.NewLine + loader.GetString("Aguarde");
                        if (isText)
                            Dialogo(v, v, c, txtResultDialogo);
                        else
                            Dialogo(v, c, v, txtResultDialogo);
                        if (foto != null) FotoVisible(); else { isP.Visibility = c; }

                    }
                    catch
                    {
                        DialogoMessage("error", "");
                    }
                }
            }
            InitializedFrostedGlass(this.GlassHost, null, 70);
            InitializedFrostedGlass(this.GlassHost2, this.GlassHost2, 70);
            InitializedFrostedGlass(this.GlassHost3, this.GlassHost3, 70);
            InitializedFrostedGlass(this.GlassHost4, this.GlassHost4, 70);
            InitializedFrostedGlass(this.TelaAbout, this.TelaAbout, 70);
            this.isP.Blur(70);
        }

        private void AddDadaBase()
        {
            if (IsFoto)
            {
                Guardados Guardados = new Guardados
                {
                    DataFoto = DateTime.Now.ToString(),
                    SymbolFoto = "",
                    NomeFoto = foto.DisplayName,
                    TextFoto = TextResult.Text
                };
                GuardadosC.Salvar(Guardados);
            }
        }

        private async void BrowserFoto_Click(object sender, RoutedEventArgs e)
        {
            txtResultDialogo = loader.GetString("preparando") + Environment.NewLine + loader.GetString("Aguarde");
            if (isText)
                Dialogo(v, v, c, txtResultDialogo);
            else
                Dialogo(v, c, v, txtResultDialogo);
            FileOpenPicker fileOpen = new FileOpenPicker();
            fileOpen.FileTypeFilter.Add(".jpg");
            fileOpen.FileTypeFilter.Add(".png");
            fileOpen.FileTypeFilter.Add(".bmp");
            fileOpen.FileTypeFilter.Add(".jpeg");
            fileOpen.SuggestedStartLocation = PickerLocationId.PicturesLibrary;
            fileOpen.ViewMode = PickerViewMode.List;
            foto = await fileOpen.PickSingleFileAsync();
            if (foto != null) FotoVisible(); else { isP.Visibility = c; ButtonEnable(true); }
        }

        private bool IsFoto
        {
            get
            {
                if (foto == null) return false;
                else return true;
            }
        }

        private async void IniciarScanner()
        {

            try
            {
                using (var stream = await foto.OpenAsync(FileAccessMode.Read))
                {
                    var decoder = await BitmapDecoder.CreateAsync(stream);

                    bitmap = await decoder.GetSoftwareBitmapAsync(BitmapPixelFormat.Bgra8, BitmapAlphaMode.Premultiplied);

                    var imgSource = new WriteableBitmap(bitmap.PixelWidth, bitmap.PixelHeight);
                    bitmap.CopyToBuffer(imgSource.PixelBuffer);
                }
                OcrEngine ocrEngine = null;
                ocrEngine = OcrEngine.TryCreateFromUserProfileLanguages();
                if (ocrEngine != null)
                {
                    var ocrResult = await ocrEngine.RecognizeAsync(bitmap);
                    if (ocrResult.Text == "")
                    {
                        if (isText)
                            Dialogo(c, v, c, txtResultDialogo);
                        else
                            Dialogo(c, c, v, txtResultDialogo);
                        DialogoMessage(loader.GetString("errorLerImagem"), AppTitle);
                    }// { AlertMessageAsync(l.GetString("imageFall")); }
                    else
                    {
                        if (TextResult.Text != "")
                        {
                            //if (btnScanner.Content.ToString() != loader.GetString("lerImagem2"))
                            //{
                            if (Model.IsHistorical && isExitScan)
                            {
                                if (ItemsLista == 0)
                                    AddDadaBase();
                                else
                                    foreach (var texto in GuardadosC.Lista())
                                    {
                                        if (TextResult.Text != texto.TextFoto)
                                            AddDadaBase();
                                    }
                            }
                            //}
                        }
                        IniciarResultadoDoScanner(ocrResult.Text);
                    }
                }
            }
            catch (Exception)
            {
            }
        }

        private void SizeTexts_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            //SettingServices.PutFontSize(SizeTexts.Value);
            TextResult.FontSize = SizeTexts.Value;
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
        }

        private async void ExceptionMessage(string ex) => await new MessageDialog(ex).ShowAsync();

        private void BtnScanner_Click(object sender, RoutedEventArgs e)
        {
            IsTap = false;
            txtResultDialogo = $"{loader.GetString("Fazendooscannernaimagem")}{Environment.NewLine}{loader.GetString("Aguarde")}";
            if (isText)
                Dialogo(v, v, c, txtResultDialogo);
            else
                Dialogo(v, c, v, txtResultDialogo);
            if (IsFoto) IniciarScanner();
        }

        private void ButtonScannerEnabled()
        {
            btnScanner.IsEnabled = IsFoto;
            if (IsFoto) btnScanner.Visibility = v;
        }

        private async void EnviarSMS(Contact Contato, string Mensagem)
        {
            var MensagemChat = new Windows.ApplicationModel.Chat.ChatMessage
            {
                Body = Mensagem
            };

            var Celular = Contato.Phones.FirstOrDefault<ContactPhone>();
            if (Celular != null)
            {
                MensagemChat.Recipients.Add(Celular.Number);

                await Windows.ApplicationModel.Chat.ChatMessageManager.ShowComposeSmsMessageAsync(MensagemChat);
            }
        }

        private void TopBarButtons_Click(object sender, RoutedEventArgs e)
        {
            if (TextResult.Text != "" && isP.Visibility == c)
            {
                Button btn = (Button)sender as Button;
                switch (btn.Tag.ToString())
                {

                    case "expandir":
                        ActionType(Type.Expand, TextResult.Text);
                        break;
                    case "save":
                        ActionType(Type.Save, TextResult.Text);
                        break;
                    case "send":
                        ActionType(Type.Send, TextResult.Text);
                        break;
                    case "mail":
                        ActionType(Type.Mail, TextResult.Text);
                        break;
                    case "share":
                        ActionType(Type.Share, TextResult.Text);
                        break;
                    case "copy":
                        ActionType(Type.Copy, TextResult.Text);
                        break;
                    case "zap":
                        ActionType(Type.Zap, TextResult.Text);
                        break;

                }
            }
            else
            {
                DialogoMessage(loader.GetString("semtext"), AppTitle);
            }

        }

        private void DataTransferManager_DataRequested(DataTransferManager sender, DataRequestedEventArgs args)
        {
            DataPackage requestData = args.Request.Data;
            requestData.Properties.Title = nameFotoNoNull;
            requestData.Properties.Description = DateTime.Now.ToString();
            requestData.SetText(shared);
        }

        private static void Notifications(string titulo, string descrition)
        {
            ToastTemplateType toastTemplate = ToastTemplateType.ToastImageAndText02;
            XmlDocument toastXml = ToastNotificationManager.GetTemplateContent(toastTemplate);
            XmlNodeList toastTextElements = toastXml.GetElementsByTagName("text");
            toastTextElements[0].AppendChild(toastXml.CreateTextNode(titulo));
            toastTextElements[1].AppendChild(toastXml.CreateTextNode(descrition));
            XmlNodeList toastImageAttributes = toastXml.GetElementsByTagName("image");
            ((XmlElement)toastImageAttributes[0]).SetAttribute("src", "ms-appx:///Assets/StoreLogo.png");
            ToastNotificationManager.CreateToastNotifier().Show(new ToastNotification(toastXml));
        }

        private void ItemLista_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Button btn = (Button)sender as Button;
                Guardados guardados = btn.DataContext as Guardados;
                IsTap = true;
                txtTittle.Content = guardados.NomeFoto;
                textContent.Content = guardados.TextFoto;
                GridController.Visibility = v;
                MenuLeitura.IsPaneOpen = !MenuLeitura.IsPaneOpen;
            }
            catch (Exception x)
            {
                DialogoMessage(x.Message, AppTitle);
            }

        }

        private bool IsTap { get; set; }

        private void Historicos_Click(object sender, RoutedEventArgs e)
        {
            MenuHistoricos.IsPaneOpen = !MenuHistoricos.IsPaneOpen;
            if (MenuHistoricos.IsPaneOpen)
                ListaUpdate();
        }

        private async void BtnDeleteHistorico_Click(object sender, RoutedEventArgs e)
        {
            if (ItemsLista != 1) { items = loader.GetString("items"); unic = loader.GetString("deletelist"); }
            else { items = loader.GetString("item"); unic = loader.GetString("unic"); }
            var dialogo = new ContentDialog()
            {
                Content = unic,
                Title = $"{loader.GetString("deletelistbtn")} {ItemsLista} {items}",
                BorderBrush = ((SolidColorBrush)App.Current.Resources["IsBorderBrush"]),
                RequestedTheme = ElementTheme.Dark,
                Background = ((SolidColorBrush)App.Current.Resources["IsBackgroundForSplitViewAndFlyout"]),
                CloseButtonText = loader.GetString("Fechar"),
                PrimaryButtonText = loader.GetString("deletelistbtn")
            };
            dialogo.PrimaryButtonClick += (s, a) =>
            {
                _dbConection db = new _dbConection();
                db._conexao.DeleteAll<Guardados>(); ListaUpdate();
            };
            await dialogo.ShowAsync();
        }

        private async void DeletaEmLista_Click(object sender, RoutedEventArgs e)
        {
            Button btn = (Button)sender as Button;
            Guardados guardados = btn.DataContext as Guardados;
            var dialogo = new ContentDialog()
            {
                Content = loader.GetString("deleteuniclist"),
                Title = AppTitle,
                BorderBrush = ((SolidColorBrush)App.Current.Resources["IsBorderBrush"]),
                RequestedTheme = ElementTheme.Dark,
                Background = ((SolidColorBrush)App.Current.Resources["IsBackgroundForSplitViewAndFlyout"]),
                CloseButtonText = loader.GetString("Fechar"),
                PrimaryButtonText = loader.GetString("deleteuniclistbtn")
            };
            dialogo.PrimaryButtonClick += (s, a) =>
            {
                Guardados g = new Guardados()
                {
                    TextFoto = guardados.TextFoto,
                    NomeFoto = guardados.NomeFoto,
                    DataFoto = guardados.DataFoto,
                    ID = guardados.ID,
                    SymbolFoto = guardados.SymbolFoto,
                };
                GuardadosC.Apagar(g);
                ListaUpdate();
            };
            await dialogo.ShowAsync();
        }

        private void Media_MediaEnded(object sender, RoutedEventArgs e)
        {
            if (!isFalante)
            {
                iconAudio.Symbol = Symbol.Play;
                texts.Symbol = Symbol.Pause;
            }
            else
            {
                texts.Symbol = Symbol.Play;
                iconAudio.Symbol = Symbol.Pause;
            }
        }

        private void TextResult_SelectionChanged(object sender, RoutedEventArgs e)
        {
            if (TextResult.SelectionLength > 0)
                Ler_txt = TextResult.SelectedText;
            else
                Ler_txt = TextResult.Text;
        }

        private void LerSelected_Click(object sender, RoutedEventArgs e)
        {

            if (TextResult.Text != "" && isP.Visibility == c)
            {
                if (Media.CurrentState == MediaElementState.Playing)
                {
                    Media.Stop();
                    texts.Symbol = Symbol.Play;
                }
                else
                {
                    if (!String.IsNullOrEmpty(Ler_txt))
                    {
                        try
                        {
                            IniciarLeituraEmVozAlta(Ler_txt);
                            isFalante = true;
                        }
                        catch (System.IO.FileNotFoundException)
                        {
                        }
                        catch (Exception x)
                        {
                            ExceptionMessage(x.Message);
                            Media.AutoPlay = false;
                        }
                    }
                    else
                    {
                        Media.Stop();
                    }
                }
            }
            else
            {
                DialogoMessage(loader.GetString("semtext"), AppTitle);
            }

        }

        private async void IniciarLeituraEmVozAlta(string ler_txt)
        {
            try
            {
                synthesizer.Options.SpeakingRate = VozVelocidade.Value;
                synthesizer.Options.AudioPitch = VozDensidade.Value;
                SpeechSynthesisStream synthesisStream = await synthesizer.SynthesizeTextToStreamAsync(ler_txt);
                Media.AutoPlay = true;
                Media.SetSource(synthesisStream, synthesisStream.ContentType);
                Media.Play();
            }
            catch { }
        }

        private void TextResult_TextChanged(object sender, TextChangedEventArgs e) => Ler_txt = TextResult.Text;

        private async void ActionType(Type type, string action)
        {
            switch (type)
            {
                case Type.Copy:
                    var dataPackage = new DataPackage();
                    dataPackage.SetText(action);
                    Clipboard.SetContent(dataPackage);
                    {
                        Notifications(AppTitle, loader.GetString("copy"));
                    }
                    break;
                case Type.Expand:
                    try
                    {
                        if (IsFoto) nameFotoNoNull = foto.DisplayName; else nameFotoNoNull = loader.GetString("NomeCriado");
                        txtTittle.Content = nameFotoNoNull;
                        textContent.Content = action;
                        GridController.Visibility = c;
                        MenuLeitura.IsPaneOpen = !MenuLeitura.IsPaneOpen;
                    }
                    catch (Exception x) { DialogoMessage(x.Message, ""); }
                    break;
                case Type.Mail:
                    try
                    {
                        objEmail.Subject = action; //Titulo do Feedback
                        await EmailManager.ShowComposeNewEmailAsync(objEmail); //Pega todas as imformações e mande o feedback
                    }
                    catch (Exception x) { DialogoMessage(x.Message, ""); }
                    break;
                case Type.Save:
                    try
                    {
                        if (IsFoto) nameFotoNoNull = foto.DisplayName; else nameFotoNoNull = loader.GetString("NomeCriado");

                        if (Model.IsCheckedBtn1)
                        {
                            FileSavePicker save = new FileSavePicker();
                            save.FileTypeChoices.Add(".txt", new List<string>() { ".txt" });
                            save.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;
                            save.SuggestedFileName = nameFotoNoNull;
                            save.DefaultFileExtension = ".txt";
                            file = await save.PickSaveFileAsync();
                        }
                        else
                        {
                            string nameFolder = AppTitle.Replace(":", "");
                            StorageFolder folder = KnownFolders.PicturesLibrary;
                            var isFolder = await folder.CreateFolderAsync(nameFolder, CreationCollisionOption.OpenIfExists);
                            file = await isFolder.CreateFileAsync($"{nameFotoNoNull}.txt", CreationCollisionOption.GenerateUniqueName);
                        }

                        if (file != null)
                        {
                            await FileIO.WriteTextAsync(file, action);
                            Notifications($"{file.Name}", $"{loader.GetString("savefile")} {file.Path}");
                        }
                    }
                    catch (Exception x)
                    {
                        await new MessageDialog(x.Message).ShowAsync();
                    }
                    break;
                case Type.Send:
                    try
                    {
                        ContactPicker contactPicker = new ContactPicker
                        {
                            CommitButtonText = loader.GetString("Selecione"),
                            SelectionMode = ContactSelectionMode.Fields
                        };
                        contactPicker.DesiredFieldsWithContactFieldType.Add(ContactFieldType.PhoneNumber);
                        contactPicker.DesiredFieldsWithContactFieldType.Add(ContactFieldType.Email);
                        IList<Contact> contatos = await contactPicker.PickContactsAsync();
                        if (contatos != null && contatos.Count > 0)
                        {
                            foreach (Contact contato in contatos)
                            {
                                ContactStore contactStore = await ContactManager.RequestStoreAsync(ContactStoreAccessType.AllContactsReadOnly);

                                if (contato != null)
                                {
                                    Contact realcontatos = await contactStore.GetContactAsync(contato.Id);

                                    EnviarSMS(realcontatos, action);
                                }
                            }
                        }
                        else
                        {
                            DialogoMessage(loader.GetString("semrealcontatos"), AppTitle);
                        }
                    }
                    catch (Exception x) { await new MessageDialog(x.Message).ShowAsync(); }
                    break;
                case Type.Zap:
                    try
                    {
                        var success = await Windows.System.Launcher.LaunchUriAsync(new Uri($"whatsapp://send?text={action}"));
                        if (!success) DialogoMessage(loader.GetString("whatsappError"), AppTitle);
                    }
                    catch
                    {
                        DialogoMessage(loader.GetString("whatsappError"), AppTitle);
                    }
                    break;
                case Type.Share:
                    try
                    {
                        shared = action;
                        if (IsFoto) nameFotoNoNull = foto.DisplayName; else nameFotoNoNull = loader.GetString("NomeCriado");
                        DataTransferManager.ShowShareUI();
                    }
                    catch { }
                    break;
                default:
                    break;
            }
        }

        private void TopBarButtons2_Click(object sender, RoutedEventArgs e)
        {
            if (textContent.Content.ToString() != "")
            {
                Button btn = (Button)sender as Button;
                switch (btn.Tag.ToString())
                {

                    case "expandir":
                        ActionType(Type.Expand, textContent.Content.ToString());
                        break;
                    case "autoler":
                        if (Media.CurrentState == MediaElementState.Playing)
                        {
                            Media.Stop();
                            iconAudio.Symbol = Symbol.Play;
                        }
                        else
                        {
                            if (!String.IsNullOrEmpty(textContent.Content.ToString()))
                            {
                                try
                                {
                                    IniciarLeituraEmVozAlta(textContent.Content.ToString());
                                    isFalante = false;
                                }
                                catch (System.IO.FileNotFoundException)
                                {
                                }
                                catch (Exception x)
                                {
                                    ExceptionMessage(x.Message);
                                    Media.AutoPlay = false;
                                }
                            }
                            else
                            {
                            }
                        }
                        break;
                    case "save":
                        ActionType(Type.Save, textContent.Content.ToString());
                        break;
                    case "send":
                        ActionType(Type.Send, textContent.Content.ToString());
                        break;
                    case "mail":
                        ActionType(Type.Mail, textContent.Content.ToString());
                        break;
                    case "share":
                        ActionType(Type.Share, textContent.Content.ToString());
                        break;
                    case "copy":
                        ActionType(Type.Copy, textContent.Content.ToString());
                        break;
                    case "zap":
                        ActionType(Type.Zap, textContent.Content.ToString());
                        break;
                    default:
                        break;
                }
            }
            else
            {
                DialogoMessage(loader.GetString("semtext"), AppTitle);
            }
        }

        private void Sobre_Click(object sender, RoutedEventArgs e) => MenuInfo.IsPaneOpen = !MenuInfo.IsPaneOpen;

        private async void HyperlinkButton_Click(object sender, RoutedEventArgs e)
        {
            HyperlinkButton link = (HyperlinkButton)sender as HyperlinkButton;
            switch (link.Tag.ToString())
            {
                case "F": await Windows.System.Launcher.LaunchUriAsync(new Uri("http://www.facebook.com/igor.dutra.3557")); break;
                case "I": await Windows.System.Launcher.LaunchUriAsync(new Uri("http://www.instagram.com/igor_sanches")); break;
                case "T": await Windows.System.Launcher.LaunchUriAsync(new Uri("http://www.twitter.com/igordutra2014")); break;
                case "W":
                    try
                    {
                        //Notifications(AppTitle, loader.GetString("zapRe")); 
                        var success = await Windows.System.Launcher.LaunchUriAsync(new Uri("whatsapp://send?phone=+5598985356501"));
                        if (!success) DialogoMessage(loader.GetString("whatsappError"), AppTitle);
                    }
                    catch
                    {
                        DialogoMessage(loader.GetString("whatsappError"), AppTitle);
                    }
                    break;
            }
        }

        private void Setting_Click(object sender, RoutedEventArgs e)
        {
            MenuSettings.IsPaneOpen = !MenuSettings.IsPaneOpen;
        }

        private void IsHistorico_Checked(object sender, RoutedEventArgs e)
        {
            SettingServices.Historical(IsHistorico.IsChecked.Value);
        }

        private void MenuHistoricos_PaneOpened(SplitView sender, object args)
        {
            ListaUpdate();
        }

        private void SaveFile_Tapped(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            RadioButton radio = sender as RadioButton;
            switch (radio.Tag.ToString())
            {
                case "Btn1":
                    SettingServices.IsCheckeds("Btn1", radio.IsChecked.Value);
                    SettingServices.IsCheckeds("Btn2", !radio.IsChecked.Value);
                    break;
                case "Btn2":
                    SettingServices.IsCheckeds("Btn1", !radio.IsChecked.Value);
                    SettingServices.IsCheckeds("Btn2", radio.IsChecked.Value);
                    break;
            }
        }

        private void Voz_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            Slider linha = sender as Slider;
            if (linha.Tag.ToString() == "Densidade")
            {
                SettingServices.IsSliders("NivelLeitura", linha.Value);
                VozDensidade.Header = Model.IsTextNivel;

            }
            else
            {
                SettingServices.IsSliders("VelocidadeLeitura", linha.Value);
                VozVelocidade.Header = Model.IsTextVelocidade;
            }
        }

        private async void Rate_Click(object sender, RoutedEventArgs e) => await Windows.System.Launcher.LaunchUriAsync(new Uri($"ms-windows-store://review/?ProductId=9NTKLB3SSPVR"));

        private void Shared_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                shared = $"{AppTitle} " + loader.GetString("LinkCompartilhar") + ": https://www.microsoft.com/store/productid/9NTKLB3SSPVR";
                nameFotoNoNull = AppTitle;
                DataTransferManager.ShowShareUI();
            }
            catch { }
        }

        private async void Feedback_Click(object sender, RoutedEventArgs e)
        {

            objEmail.Subject = AppTitle + "... V: " + Model.Version;
            objEmail.To.Add(new EmailRecipient("igordeveloper18@outlook.com"));
            await EmailManager.ShowComposeNewEmailAsync(objEmail);
        }

        private void Media_MediaOpened(object sender, RoutedEventArgs e)
        {

            if (!isFalante)
            {
                iconAudio.Symbol = Symbol.Pause;
                texts.Symbol = Symbol.Play;
            }
            else
            {
                texts.Symbol = Symbol.Pause;
                iconAudio.Symbol = Symbol.Play;
            }
        }

        private void Media_MediaFailed(object sender, ExceptionRoutedEventArgs e)
        {
            if (!isFalante)
            {
                iconAudio.Symbol = Symbol.Play;
                texts.Symbol = Symbol.Pause;
            }
            else
            {
                texts.Symbol = Symbol.Play;
                iconAudio.Symbol = Symbol.Pause;
            }
        }

        private void Font_Click(object sender, RoutedEventArgs e) => FontFaceMenuFlyout.ShowAt(this);
    }
}
