using ListaComprasApp.ViewModels;
using ListaComprasApp.Services;
using ListaComprasApp.Models;

namespace ListaComprasApp.Views;

public partial class ProdutosPadraoPage : ContentPage
{
    private readonly ProdutosPadraoViewModel _viewModel;
    private readonly Dictionary<string, int> _quantidades = new();
    private readonly Dictionary<string, Entry> _valoresUnitarios = new();
    private Label _totalLabel;

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
                    Padding = 15,
                    Margin = new Thickness(0, 2)
                };

                var itemGrid = new Grid
                {
                    ColumnDefinitions =
                    {
                        new ColumnDefinition { Width = GridLength.Auto }, // Checkbox
                        new ColumnDefinition { Width = GridLength.Auto }, // Ícone
                        new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) }, // Nome + detalhes
                        new ColumnDefinition { Width = new GridLength(100, GridUnitType.Absolute) }, // Controles quantidade
                        new ColumnDefinition { Width = new GridLength(80, GridUnitType.Absolute) }  // Preço
                    }
                };

                // Checkbox
                var checkbox = new CheckBox
                {
                    VerticalOptions = LayoutOptions.Center
                };
                itemGrid.Children.Add(checkbox);
                Grid.SetColumn(checkbox, 0);

                // Ícone
                var iconeLabel = new Label
                {
                    Text = produto.Icone,
                    FontSize = 24,
                    VerticalOptions = LayoutOptions.Center,
                    Margin = new Thickness(10, 0, 0, 0)
                };
                itemGrid.Children.Add(iconeLabel);
                Grid.SetColumn(iconeLabel, 1);

                // Nome e detalhes
                var detalhesStack = new StackLayout
                {
                    VerticalOptions = LayoutOptions.Center,
                    Margin = new Thickness(10, 0, 5, 0),
                    Spacing = 2
                };

                var nomeLabel = new Label
                {
                    Text = produto.Nome.ToUpper(),
                    FontSize = 16,
                    FontAttributes = FontAttributes.Bold,
                    LineBreakMode = LineBreakMode.WordWrap,
                    MaxLines = 2
                };
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

                var detalhesLabel = new Label
                {
                    Text = unidadeTexto,
                    FontSize = 10,
                    TextColor = Colors.Gray
                };
                detalhesStack.Children.Add(detalhesLabel);

                // Inicializar quantidade para este produto
                _quantidades[produto.Nome] = 1;

                // Criar label de preço
                var precoLabel = new Label
                {
                    Text = $"R$ {produto.PrecoMedio:F2}",
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
                    Text = produto.PrecoMedio.ToString("F2"),
                    FontSize = 10,
                    Keyboard = Keyboard.Numeric,
                    WidthRequest = 60,
                    HeightRequest = 25,
                    Margin = new Thickness(0, 2, 0, 0)
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
                                var expectedText = formattedValue.ToString("F2");

                                entry.Text = expectedText;
                                entry.CursorPosition = expectedText.Length;

                                // Atualiza o preço total do item
                                var quantidade = _quantidades[produtoAtual.Nome];
                                precoLabel.Text = $"R$ {formattedValue * quantidade:F2}";
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

                detalhesStack.Children.Add(valorEntry);
                itemGrid.Children.Add(detalhesStack);
                Grid.SetColumn(detalhesStack, 2);

                // Controles de quantidade
                var quantidadeStack = new StackLayout
                {
                    Orientation = StackOrientation.Horizontal,
                    VerticalOptions = LayoutOptions.Center,
                    HorizontalOptions = LayoutOptions.Center,
                    Spacing = 5
                };

                var diminuirButton = new Button
                {
                    Text = "-",
                    FontSize = 16,
                    FontAttributes = FontAttributes.Bold,
                    BackgroundColor = Colors.LightCoral,
                    TextColor = Colors.White,
                    WidthRequest = 25,
                    HeightRequest = 25,
                    CornerRadius = 12,
                    Padding = 0
                };

                var quantidadeLabel = new Label
                {
                    Text = "1",
                    FontSize = 14,
                    FontAttributes = FontAttributes.Bold,
                    VerticalOptions = LayoutOptions.Center,
                    HorizontalTextAlignment = TextAlignment.Center,
                    WidthRequest = 25
                };

                var aumentarButton = new Button
                {
                    Text = "+",
                    FontSize = 16,
                    FontAttributes = FontAttributes.Bold,
                    BackgroundColor = Colors.LightGreen,
                    TextColor = Colors.White,
                    WidthRequest = 25,
                    HeightRequest = 25,
                    CornerRadius = 12,
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
                        precoLabel.Text = $"R$ {valorUnitario * _quantidades[produtoAtual.Nome]:F2}";
                        AtualizarTotalGeral();
                    }
                };

                aumentarButton.Clicked += (s, e) =>
                {
                    _quantidades[produtoAtual.Nome]++;
                    quantidadeLabel.Text = _quantidades[produtoAtual.Nome].ToString();
                    var valorUnitario = ObterValorUnitario();
                    precoLabel.Text = $"R$ {valorUnitario * _quantidades[produtoAtual.Nome]:F2}";
                    AtualizarTotalGeral();
                };

                quantidadeStack.Children.Add(diminuirButton);
                quantidadeStack.Children.Add(quantidadeLabel);
                quantidadeStack.Children.Add(aumentarButton);

                itemGrid.Children.Add(quantidadeStack);
                Grid.SetColumn(quantidadeStack, 3);

                // Preço
                itemGrid.Children.Add(precoLabel);
                Grid.SetColumn(precoLabel, 4);

                itemFrame.Content = itemGrid;
                stackLayout.Children.Add(itemFrame);
            }
        }

        // Total da lista
        _totalLabel = new Label
        {
            Text = $"TOTAL: R$ {produtos.Sum(p => p.PrecoMedio):F2}",
            FontSize = 20,
            FontAttributes = FontAttributes.Bold,
            TextColor = Colors.Green,
            HorizontalOptions = LayoutOptions.Center,
            Margin = new Thickness(0, 20, 0, 0)
        };
        stackLayout.Children.Add(_totalLabel);

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
        // Navegar para a aba de listas para permitir edição
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
            _totalLabel.Text = $"TOTAL: R$ {totalGeral:F2}";
        }
    }
}