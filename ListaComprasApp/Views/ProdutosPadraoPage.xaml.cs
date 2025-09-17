using ListaComprasApp.ViewModels;
using ListaComprasApp.Services;
using ListaComprasApp.Models;
using System.Globalization;

namespace ListaComprasApp.Views;

public partial class ProdutosPadraoPage : ContentPage
{
    private readonly ProdutosPadraoViewModel _viewModel;
    private readonly Dictionary<string, int> _quantidades = new();
    private readonly Dictionary<string, Entry> _valoresUnitarios = new();
    private readonly Dictionary<string, CheckBox> _checkboxes = new();
    private Label _totalLabel;
    private Label _totalCheckadosLabel;

    public ProdutosPadraoPage(ProdutosPadraoViewModel viewModel)
    {
        _viewModel = viewModel;
        BindingContext = viewModel;

        Title = "Lista de Compras";

        CreateUI();
        LoadProducts();
    }

    private void CreateUI()
    {
        // Definir cultura brasileira para toda a UI
        CultureInfo.DefaultThreadCurrentCulture = new CultureInfo("pt-BR");
        CultureInfo.DefaultThreadCurrentUICulture = new CultureInfo("pt-BR");

        var scrollView = new ScrollView();
        var stackLayout = new StackLayout { Padding = 20 };

        // Header da lista
        var headerLabel = new Label
        {
            Text = "Lista de Compras Padrão",
            FontSize = 20,
            FontAttributes = FontAttributes.Bold,
            HorizontalOptions = LayoutOptions.Center,
            Margin = new Thickness(0, 0, 0, 20)
        };
        stackLayout.Children.Add(headerLabel);

        // Mostrar produtos como lista de compras
        var produtos = ProdutosPadraoService.ObterTodosProdutos();
        var categorias = produtos.GroupBy(p => p.Categoria);

        foreach (var categoria in categorias)
        {
            // Título da categoria
            var categoriaTexto = categoria.Key switch
            {
                Categoria.FrutasVerduras => "Frutas e Verduras",
                Categoria.Carnes => "Carnes",
                Categoria.Laticinios => "Laticínios",
                Categoria.Bebidas => "Bebidas",
                Categoria.Limpeza => "Limpeza",
                Categoria.Padaria => "Padaria",
                Categoria.Congelados => "Congelados",
                Categoria.Higiene => "Higiene",
                _ => "Outros"
            };

            var categoriaLabel = new Label
            {
                Text = categoriaTexto,
                FontSize = 16,
                FontAttributes = FontAttributes.Bold,
                TextColor = Colors.Purple,
                Margin = new Thickness(0, 15, 0, 5)
            };
            stackLayout.Children.Add(categoriaLabel);

            // Itens da categoria
            foreach (var produto in categoria)
            {
                var itemFrame = new Frame
                {
                    BackgroundColor = Colors.White,
                    BorderColor = Colors.LightGray,
                    CornerRadius = 8,
                    Padding = new Thickness(15, 8, 15, 8),
                    Margin = new Thickness(0, 2),
                    HeightRequest = 60
                };

                var itemGrid = new Grid
                {
                    ColumnDefinitions =
                    {
                        new ColumnDefinition { Width = GridLength.Auto }, // Checkbox 
                        new ColumnDefinition { Width = GridLength.Auto }, // Ícone 
                        new ColumnDefinition { Width = new GridLength(4, GridUnitType.Star) }, // Nome + detalhes
                        new ColumnDefinition { Width = new GridLength(90, GridUnitType.Absolute) }, // Controles quantidade 
                        new ColumnDefinition { Width = new GridLength(75, GridUnitType.Absolute) }  // Preço (aumentado para evitar quebras)
                    }
                };

                // Checkbox
                var checkbox = new CheckBox
                {
                    VerticalOptions = LayoutOptions.Center
                };
                _checkboxes[produto.Nome] = checkbox;
                checkbox.CheckedChanged += (s, e) => AtualizarTotalCheckados();
                itemGrid.Children.Add(checkbox);
                Grid.SetColumn(checkbox, 0);

                // Ícone
                var iconeLabel = new Label
                {
                    Text = produto.Icone,
                    FontSize = 24,
                    VerticalOptions = LayoutOptions.Center,
                    HorizontalOptions = LayoutOptions.Center,
                    Margin = new Thickness(5, 0, 5, 0)
                };
                itemGrid.Children.Add(iconeLabel);
                Grid.SetColumn(iconeLabel, 1);

                // Inicializar quantidade para este produto
                _quantidades[produto.Nome] = 1;

                // Nome e detalhes
                var detalhesStack = new StackLayout
                {
                    VerticalOptions = LayoutOptions.Start,
                    HorizontalOptions = LayoutOptions.FillAndExpand,
                    Margin = new Thickness(5, 0, 10, 0),
                    Spacing = 2
                };

                // Verificar se é palavra única ou composta
                var nomeUpper = produto.Nome.ToUpper();
                var temEspacos = nomeUpper.Contains(' ');

                var nomeLabel = new Label
                {
                    Text = nomeUpper,
                    FontSize = 14,
                    FontAttributes = FontAttributes.Bold,
                    LineBreakMode = temEspacos ? LineBreakMode.WordWrap : LineBreakMode.TailTruncation,
                    MaxLines = temEspacos ? 2 : 1,
                    VerticalOptions = LayoutOptions.Start,
                    HorizontalOptions = LayoutOptions.FillAndExpand
                };

                // Ajustar fonte para palavras longas
                if (!temEspacos && nomeUpper.Length > 10)
                {
                    nomeLabel.FontSize = 12;
                }
                if (!temEspacos && nomeUpper.Length > 15)
                {
                    nomeLabel.FontSize = 10;
                }
                detalhesStack.Children.Add(nomeLabel);

                var unidadeTexto = produto.Unidade switch
                {
                    UnidadeMedida.Kilo => "kg",
                    UnidadeMedida.Grama => "g",
                    UnidadeMedida.Litro => "L",
                    UnidadeMedida.Unidade => "un",
                    UnidadeMedida.Pacote => "pct",
                    UnidadeMedida.Caixa => "cx",
                    _ => "un"
                };

                // MODIFICAÇÃO: Unidade e quantidade em um layout horizontal
                var unidadeQuantidadeStack = new StackLayout
                {
                    Orientation = StackOrientation.Horizontal,
                    Spacing = 5
                };

                var detalhesLabel = new Label
                {
                    Text = unidadeTexto,
                    FontSize = 10,
                    TextColor = Colors.Gray,
                    VerticalOptions = LayoutOptions.Center
                };
                unidadeQuantidadeStack.Children.Add(detalhesLabel);

                // Adicionar a quantidade após a unidade
                var quantidadeLabel = new Label
                {
                    Text = _quantidades[produto.Nome].ToString(),
                    FontSize = 10,
                    FontAttributes = FontAttributes.Bold,
                    TextColor = Colors.Gray,
                    VerticalOptions = LayoutOptions.Center
                };
                unidadeQuantidadeStack.Children.Add(quantidadeLabel);

                // Adicionar o stack de unidade+quantidade ao stack de detalhes
                detalhesStack.Children.Add(unidadeQuantidadeStack);

                // Adicionar stack de detalhes ao grid
                itemGrid.Children.Add(detalhesStack);
                Grid.SetColumn(detalhesStack, 2);

                // Criar label de preço com formatação de moeda brasileira
                var precoLabel = new Label
                {
                    Text = string.Format(CultureInfo.GetCultureInfo("pt-BR"), "R$ {0:N2}", produto.PrecoMedio * _quantidades[produto.Nome]),
                    FontSize = 14,
                    FontAttributes = FontAttributes.Bold,
                    TextColor = Colors.Green,
                    VerticalOptions = LayoutOptions.Center,
                    HorizontalTextAlignment = TextAlignment.End
                };

                // Campo de entrada para valor unitário
                var valorEntry = new Entry
                {
                    Placeholder = "0,00",
                    Text = produto.PrecoMedio.ToString("N2", CultureInfo.GetCultureInfo("pt-BR")),
                    FontSize = 12,
                    Keyboard = Keyboard.Numeric,
                    WidthRequest = 65,
                    HeightRequest = 50,
                    HorizontalOptions = LayoutOptions.Center,
                    VerticalOptions = LayoutOptions.Center, // Centralizado verticalmente
                    HorizontalTextAlignment = TextAlignment.Center,
                    Margin = new Thickness(0, -8, 0, 0) // Margem negativa no topo para subir o campo
                };

                // Armazenar referência do Entry
                _valoresUnitarios[produto.Nome] = valorEntry;

                // Capturar produto atual para uso nos eventos
                var produtoAtual = produto;
                var isUserInput = false;

                // Selecionar todo o texto quando ganhar foco
                valorEntry.Focused += (s, e) =>
                {
                    var entry = s as Entry;
                    if (entry != null)
                    {
                        Device.BeginInvokeOnMainThread(() =>
                        {
                            entry.CursorPosition = 0;
                            entry.SelectionLength = entry.Text?.Length ?? 0;
                        });
                    }
                };

                // Limpar seleção quando começar a digitar
                valorEntry.TextChanged += (s, e) =>
                {
                    var entry = s as Entry;
                    if (entry != null)
                    {
                        // Evitar processamento durante formatação automática
                        if (isUserInput) return;
                        isUserInput = true;

                        // Remove caracteres não numéricos
                        var numericText = new string(entry.Text.Where(char.IsDigit).ToArray());

                        if (!string.IsNullOrEmpty(numericText))
                        {
                            // Converte para decimal considerando centavos
                            if (long.TryParse(numericText, out long value))
                            {
                                var formattedValue = (decimal)value / 100;

                                var expectedText = formattedValue.ToString("N2", CultureInfo.GetCultureInfo("pt-BR"));

                                entry.Text = expectedText;
                                entry.CursorPosition = expectedText.Length;

                                // Atualiza o preço total do item
                                var quantidade = _quantidades[produtoAtual.Nome];
                                precoLabel.Text = string.Format(CultureInfo.GetCultureInfo("pt-BR"), "R$ {0:N2}", formattedValue * quantidade);
                                AtualizarTotalGeral();
                            }
                        }
                        else
                        {
                            entry.Text = "0,00";
                            precoLabel.Text = "R$ 0,00";
                            AtualizarTotalGeral();
                        }

                        isUserInput = false;
                    }
                };

                // Container horizontal para o Entry e botões verticais
                var quantidadeContainer = new StackLayout
                {
                    Orientation = StackOrientation.Horizontal,
                    VerticalOptions = LayoutOptions.Center,
                    HorizontalOptions = LayoutOptions.Start,
                    Spacing = 5,
                    Padding = new Thickness(0, 0, 0, 0)
                };

                // Adicionar o Entry à esquerda
                quantidadeContainer.Children.Add(valorEntry);

                // Container vertical para os botões
                var botoesPlusMinusStack = new StackLayout
                {
                    Orientation = StackOrientation.Vertical,
                    VerticalOptions = LayoutOptions.Center,
                    HorizontalOptions = LayoutOptions.Start,
                    Spacing = 2,
                    Margin = new Thickness(0, 0, 0, 0)
                };

                // Botões de controle
                var aumentarButton = new Button
                {
                    Text = "+",
                    FontSize = 12,
                    FontAttributes = FontAttributes.Bold,
                    BackgroundColor = Colors.LightGreen,
                    TextColor = Colors.White,
                    WidthRequest = 20,
                    HeightRequest = 20,
                    CornerRadius = 10,
                    Padding = 0
                };

                var diminuirButton = new Button
                {
                    Text = "-",
                    FontSize = 12,
                    FontAttributes = FontAttributes.Bold,
                    BackgroundColor = Colors.LightCoral,
                    TextColor = Colors.White,
                    WidthRequest = 20,
                    HeightRequest = 20,
                    CornerRadius = 10,
                    Padding = 0
                };

                // Função para obter valor unitário atual
                decimal ObterValorUnitario()
                {
                    if (_valoresUnitarios.ContainsKey(produtoAtual.Nome))
                    {
                        var entry = _valoresUnitarios[produtoAtual.Nome];
                        if (decimal.TryParse(entry.Text, out decimal valor))
                            return valor;
                    }
                    return 0;
                }

                // Eventos dos botões
                diminuirButton.Clicked += (s, e) =>
                {
                    if (_quantidades[produtoAtual.Nome] > 1)
                    {
                        _quantidades[produtoAtual.Nome]--;
                        quantidadeLabel.Text = _quantidades[produtoAtual.Nome].ToString();
                        var valorUnitario = ObterValorUnitario();
                        precoLabel.Text = string.Format(CultureInfo.GetCultureInfo("pt-BR"), "R$ {0:N2}", valorUnitario * _quantidades[produtoAtual.Nome]);
                        AtualizarTotalGeral();
                    }
                };

                aumentarButton.Clicked += (s, e) =>
                {
                    _quantidades[produtoAtual.Nome]++;
                    quantidadeLabel.Text = _quantidades[produtoAtual.Nome].ToString();
                    var valorUnitario = ObterValorUnitario();
                    precoLabel.Text = string.Format(CultureInfo.GetCultureInfo("pt-BR"), "R$ {0:N2}", valorUnitario * _quantidades[produtoAtual.Nome]);
                    AtualizarTotalGeral();
                };

                // Adicionar os botões ao stack vertical
                botoesPlusMinusStack.Children.Add(aumentarButton);
                botoesPlusMinusStack.Children.Add(diminuirButton);

                // Adicionar o stack de botões ao container horizontal
                quantidadeContainer.Children.Add(botoesPlusMinusStack);

                // Adicionar o container ao Grid
                itemGrid.Children.Add(quantidadeContainer);
                Grid.SetColumn(quantidadeContainer, 3);

                // Container para preço
                var precoStack = new StackLayout
                {
                    VerticalOptions = LayoutOptions.Center,
                    HorizontalOptions = LayoutOptions.End,
                    Spacing = 2
                };

                // Adicionar o preço
                precoStack.Children.Add(precoLabel);

                // Adicionar o stack de preço ao Grid
                itemGrid.Children.Add(precoStack);
                Grid.SetColumn(precoStack, 4);

                itemFrame.Content = itemGrid;
                stackLayout.Children.Add(itemFrame);
            }
        }

        // Total da lista
        _totalLabel = new Label
        {
            Text = string.Format(CultureInfo.GetCultureInfo("pt-BR"), "TOTAL: R$ {0:N2}", produtos.Sum(p => p.PrecoMedio)),
            FontSize = 20,
            FontAttributes = FontAttributes.Bold,
            TextColor = Colors.Green,
            HorizontalOptions = LayoutOptions.Center,
            Margin = new Thickness(0, 20, 0, 0)
        };
        stackLayout.Children.Add(_totalLabel);

        // Total dos itens marcados
        _totalCheckadosLabel = new Label
        {
            Text = "TOTAL COMPRADO: R$ 0,00",
            FontSize = 18,
            FontAttributes = FontAttributes.Bold,
            TextColor = Colors.Blue,
            HorizontalOptions = LayoutOptions.Center,
            Margin = new Thickness(0, 10, 0, 10)
        };
        stackLayout.Children.Add(_totalCheckadosLabel);

        // Botões de ação
        var botoesStack = new StackLayout
        {
            Orientation = StackOrientation.Horizontal,
            HorizontalOptions = LayoutOptions.Center,
            Margin = new Thickness(0, 20, 0, 0)
        };

        var editarButton = new Button
        {
            Text = "Editar Lista",
            FontSize = 16,
            BackgroundColor = Colors.Orange,
            TextColor = Colors.White,
            Margin = new Thickness(0, 0, 10, 0),
            CornerRadius = 8
        };
        editarButton.Clicked += OnEditarListaClicked;

        var finalizarButton = new Button
        {
            Text = "Finalizar Compra",
            FontSize = 16,
            BackgroundColor = Colors.Green,
            TextColor = Colors.White,
            CornerRadius = 8
        };
        finalizarButton.Clicked += OnFinalizarCompraClicked;

        botoesStack.Children.Add(editarButton);
        botoesStack.Children.Add(finalizarButton);
        stackLayout.Children.Add(botoesStack);

        scrollView.Content = stackLayout;
        Content = scrollView;
    }

    private void LoadProducts()
    {
        // Os produtos já são carregados na CreateUI
    }

    private async void OnEditarListaClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("//main");
    }

    private async void OnFinalizarCompraClicked(object sender, EventArgs e)
    {
        var result = await DisplayAlert("Finalizar Compra",
            "Deseja finalizar esta lista de compras?",
            "Sim", "Não");

        if (result)
        {
            await DisplayAlert("Sucesso",
                "Lista finalizada! Uma nova lista padrão será criada.",
                "OK");
        }
    }

    private void AtualizarTotalGeral()
    {
        decimal totalGeral = 0;

        foreach (var kvp in _quantidades)
        {
            var nomeProduto = kvp.Key;
            var quantidade = kvp.Value;

            if (_valoresUnitarios.ContainsKey(nomeProduto))
            {
                var valorEntry = _valoresUnitarios[nomeProduto];
                if (decimal.TryParse(valorEntry.Text, out decimal valorUnitario))
                {
                    totalGeral += valorUnitario * quantidade;
                }
            }
        }

        if (_totalLabel != null)
        {
            _totalLabel.Text = string.Format(CultureInfo.GetCultureInfo("pt-BR"), "TOTAL: R$ {0:N2}", totalGeral);
        }

        AtualizarTotalCheckados();
    }

    private void AtualizarTotalCheckados()
    {
        decimal totalCheckados = 0;

        foreach (var kvp in _checkboxes)
        {
            var nomeProduto = kvp.Key;
            var checkbox = kvp.Value;

            if (checkbox.IsChecked)
            {
                var quantidade = _quantidades[nomeProduto];

                if (_valoresUnitarios.ContainsKey(nomeProduto))
                {
                    var valorEntry = _valoresUnitarios[nomeProduto];
                    if (decimal.TryParse(valorEntry.Text, out decimal valorUnitario))
                    {
                        totalCheckados += valorUnitario * quantidade;
                    }
                }
            }
        }

        if (_totalCheckadosLabel != null)
        {
            _totalCheckadosLabel.Text = string.Format(CultureInfo.GetCultureInfo("pt-BR"), "TOTAL SELECIONADOS: R$ {0:N2}", totalCheckados);
        }
    }
}