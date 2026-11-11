# Unisaki Posing System

Sistema de poses comum ao Kawaii Posing, Cool Posing e outras ferramentas. Utiliza Modular Avatar e NDMF para gerar menus e animações necessários ao carregar o avatar.

Este README destina-se à versão v3.2.0 em desenvolvimento. A data oficial de lançamento ainda não foi definida. A versão instalada aparece no topo do Inspector. Consulte também as [novidades e alterações](CHANGELOG.md).

## Idioma de apresentação e documentos offline

Escolha o idioma em «Language / 言語», no topo do Inspector. Uma única escolha aplica-se ao Inspector e aos menus VRChat gerados. Carregue novamente o avatar para mudar o idioma dentro do jogo.

Sem escolha explícita, é usado o idioma do computador; idiomas não suportados utilizam inglês. São suportados Windows, macOS e Linux. A última escolha é guardada em EditorPrefs e reutilizada noutros projetos do mesmo utilizador do sistema neste computador. Todas as ferramentas de poses e todos os avatares usam o mesmo idioma. Alterá-lo no Inspector de qualquer ferramenta atualiza a preferência comum; as escolhas de idioma por avatar são ignoradas. Não existe sincronização com outros computadores. O armazenamento da licença não é alterado. O botão de idioma aparece apenas na confirmação inicial da instalação quando não há um idioma guardado. As alterações posteriores são feitas no Inspector.

Escolha o idioma do README, histórico e manual na [lista de idiomas](../index.html) incluída. Os atalhos do Inspector para manual e alterações seguem a seleção. Os termos originais e as traduções de referência continuam disponíveis em separado.

## Mudança de postura no modo desktop

Foi reduzido o atraso até aplicar a pose escolhida ao alternar entre em pé, agachado e sentado no desktop. Funciona na compilação normal e no pré-cálculo, sem comandos especiais ou opções adicionais. Em VR e situações especiais mantém-se o processamento anterior. Atualize pré-cálculos antigos antes de carregar.

## Instalação e atualização

Adicione a ferramenta através de VCC ou ALCOM, seguindo os ficheiros e instruções da compra. Este sistema é instalado como dependência da ferramenta de poses.

Requisitos da versão v3.2.0:

- Projeto de avatar VRChat com Unity 2022.3
- VRChat Avatars SDK 3.10.4 ou posterior
- Modular Avatar da série 1.x, versão 1.15.1 ou posterior
- NDMF (Non-Destructive Modular Framework) da série 1.x, versão 1.14.0 ou posterior

Se utiliza Kawaii Posing, atualize para 3.2.0 em ALCOM / VCC e confirme que Unisaki Posing System também está em 3.2.0. Se usava pré-cálculo, prima «Atualizar pré-cálculo», guarde a cena e carregue o avatar.

Cool Posing 1.2.0 utiliza o sistema comum 3.2.0. Atualize o produto e o sistema comum para as versões correspondentes.

### Para utilizadores da versão 8bit

Novas instalações utilizam apenas o Prefab normal. Desative funções desnecessárias para reduzir o consumo. A seleção de poses, por si só, utiliza 8 bit de base.

Os antigos Prefabs 8bit permanecem em «Editor/Legacy» de cada produto para preservar referências de avatares e predefinições existentes. Não é preciso eliminar ou substituir um Prefab para continuar a usar a versão 8bit já instalada. O menu de adição do Unity deixou de oferecer essa versão.

## Organização das definições

Selecione 「可愛いポーズ」, 「カッコいいポーズ」 ou o objeto equivalente dentro do avatar na Hierarchy.

| Separador | Conteúdo |
| --- | --- |
| Funções | Itens do menu do jogo, funções práticas, ícones e consumo de ExpressionParameters |
| Poses | Seleção, adição e ordem de poses, pré-visualização na Scene, detalhes e ajuste de animação |
| Configuração do avatar | Correções de proporções e joelhos, importação de poses originais ou movimentos próprios e predefinições |

Nas opções com detalhes, o comando para abrir definições mostra os campos abaixo da opção. A respiração também se configura em «Funções».

«Ajuda / Alterações», no topo, abre manual, assistência e histórico. Pode alternar entre o histórico do produto e o de Unisaki Posing System.

## Funções e consumo de ExpressionParameters

As escolhas em «Funções > Funções do menu» aparecem dentro do jogo. Por exemplo, Espelhar permite ativar e desativar o efeito em VRChat; selecioná-lo no Inspector não mantém obrigatoriamente todas as poses espelhadas.

| Função | Bits adicionais |
| --- | ---: |
| Espelhar | 1 bit |
| Sem gestos | 1 bit |
| Animar respiração | 0 bit |
| Respiração: ativação no jogo | 1 bit |
| Altura pés | 8 bit |
| Altura chão | 8 bit |
| Modo foto apenas local | 0 bit |
| Sincronizar Modo foto | 16 bit |
| Manter cabeça visível no Modo foto | 0 bit |
| Desativar balanço em repouso do VRChat | 0 bit |
| Reduzir deformação por rotação da cabeça | 1 bit (partilhado com Virar) |
| Virar | 10 bit (1 bit partilhado com a correção de deformação) |

Virar utiliza 1 bit para ativação, 8 bit para inclinação da cabeça e 1 bit para bloquear o pescoço. Se a correção de deformação já estiver ativa, adicionar Virar aumenta o consumo em 9 bit. A parte partilhada só é contada uma vez; a indicação acompanha as escolhas.

O total mostrado refere-se às ferramentas de poses instaladas no avatar, não ao avatar inteiro com roupa e outros sistemas. Opções comuns de várias ferramentas não são contadas duas vezes. Se o número de poses exceder o intervalo básico, a seleção também necessita de bits adicionais.

### Altura pés e Altura chão

- «Altura pés»: ajusta a altura do lado dos pés, mantendo a cabeça no lugar. Utilize para alinhar os pés com chão ou cama.
- «Altura chão»: desloca todo o corpo, incluindo a cabeça. 50% é a altura original, 0% corresponde a 3m abaixo e 100% a 3m acima. O valor inicial é 50%.

Ambas estão em «Definições» no jogo, Altura chão depois de Altura pés. Afastar a altura do chão do centro desloca o corpo verticalmente. Voltar perto de 50% repõe a altura original e termina a correção. Não há um interruptor de flutuação separado. A vista é corrigida no início da flutuação e enquanto se altera a altura.

Para animações normais, a altura dos pés deixou de duplicar os movimentos do corpo inteiro. Isto reduz os dados gerados, mas não significa que o tamanho total de qualquer avatar passe automaticamente a um terço.

### Modo foto

1. Ative «Funções > Funções do menu > Modo foto».
2. Em VRChat abra Modo foto e selecione uma pose em «Escolher pose».
3. «Distância» avança até 3m e «Direção» roda o avatar. A uma distância superior a 0, o avatar olha para si.
4. Ative «Fixar pés» quando necessário. Não é imposto no início, para permitir ajustar a posição primeiro. Se adicionou Sem gestos, também pode mudar as mãos no mesmo ecrã.
5. «Sair da foto» repõe distância e direção e liberta os pés.

Movimento e rotação do Modo foto utilizam 0 bit apenas na sua vista. Para mostrar a posição e direção aos outros, ative «Funções práticas > Sincronizar Modo foto». Ambas utilizam 16 bit no total.

Se a cabeça do seu avatar próximo desaparecer, ative «Funções práticas > Manter cabeça visível no Modo foto». Aplica-se só à sua vista durante as fotos; ao terminar regressa a apresentação normal.

### Mudar de pose deitada ao saltar

Com poses de costas e de bruços ambas selecionadas, pode alterná-las saltando no lugar. Com apenas uma orientação, mantém essa pose. Para desativar, retire «Trocar poses deitadas pulando» em «Funções → Funções práticas» no Unity. O consumo sincronizado adicional é 0 bit. Com várias ferramentas, desativar numa aplica-se a todo o avatar.

### Virar

Nas poses deitadas compatíveis, inclinar a cabeça para os lados faz o corpo virar suavemente. A função não torna automaticamente todas as poses compatíveis.

1. Ative «Funções > Funções do menu > Virar».
2. Em Poses, abra os Detalhes da pose desejada e ative «Ativar Virar».
3. Atribua uma animação lateral a «Pose lateral para virar». A pose original e a lateral geram as viragens para ambos os lados. Ajuste a «Intensidade da correção do chão» se necessário.
4. Em VRChat escolha essa pose e ative Virar, com o pescoço desbloqueado.

Ao usar Virar, a inclinação da cabeça determina a posição de reprodução, tornando indisponível o MotionTime normal. Para uma animação completa já preparada, atribua-a ao campo normal e deixe Pose lateral para virar vazia. Prepare esquerda em 0, costas em 0.5 e direita em 1.

### Respiração e balanço normal

«Funções > Funções práticas > Animar respiração» move o peito e outras partes ao respirar. Abra as definições e escolha Nenhum, Fraco, Médio, Forte, Rápido ou Personalizado por categoria. Rápido é respiração acelerada; Personalizado permite escolher a sua animação.

O movimento de respiração sozinho utiliza 0 bit. Para o parar e reiniciar em VRChat, ative também «Funções do menu > Respiração». A sincronização da ativação utiliza 1 bit. Mudar a intensidade não aumenta os bits.

Pescoço, ombros e braços acompanham o peito para reduzir deslocações da cabeça e mãos. Não é uma fixação total: escolha uma intensidade adequada às proporções e pose.

«Funções práticas > Desativar balanço em repouso do VRChat» reduz o movimento corporal que VRChat acrescenta normalmente em repouso. Está desativado inicialmente e conserva a respiração da ferramenta e movimentos personalizados. Pode ser usado sem respiração e não acrescenta bits. Não elimina movimentos causados por outras fontes.

Com várias ferramentas, desativar o balanço padrão numa aplica-se ao avatar inteiro. Para o repor, desative esta opção em todas e carregue novamente.

### Reduzir deformação por rotação da cabeça

Utilize se a pose deformar ao libertar o pescoço porque a direção difere da original. Está em «Funções > Funções práticas» e não acrescenta menu próprio. Com o pescoço fixo utiliza a direção original. As poses de costas estão excluídas para conservar a orientação inicial para cima.

### Ícones do menu

- «Gerar ícones das poses»: cria imagens do avatar em cada pose e apresenta-as no menu. Facilita reconhecer poses, mas prolonga a compilação devido às imagens.
- «Reduzir tamanho dos ícones»: baixa a resolução para reduzir o tamanho do avatar.

Android não gera ícones de poses. Utilize «Refazer ícones» depois de alterar roupa, por exemplo. As imagens da lista são atualizadas com «Atualizar miniaturas das poses». Leia a confirmação antes de executar, pois pode demorar.

## Adicionar, ordenar e pré-visualizar poses

Escolha a categoria à esquerda de Poses. A lupa superior pesquisa nomes entre todas as categorias. Detalhes reúne animação, ícone, MotionTime e restantes opções.

Adicione várias animações arrastando-as para a área de adição ou selecione-as em Project e prima «Importar clips selecionados em Project». Referências duplicadas ao mesmo clip são removidas.

Mude a posição de destino em «Detalhes > Ordem» ou use os botões para cima/baixo. Os detalhes mantêm-se abertos ou fechados após mover.

Pré-visualização mostra a pose na Scene sem abrir a janela de ajuste. Ao terminar, regressa a imagem original. Esta visualização simples não executa MA ou NDMF, podendo diferir da roupa integrada ou FloorAdjuster. Verifique roupa e contacto com o chão também após compilar o avatar.

## Ajuste de animação e ajuste automático dos membros (IK)

Abra «Poses > Ajuste de animação» para ajustar corpo e membros com pré-visualização. No fim, guarde a animação de ajuste usando os comandos da janela.

Em produtos e poses compatíveis, o ajuste automático adapta os membros às proporções. «Ativar ajuste automático dos membros» controla o conjunto; «Ativar IK para esta pose» controla cada pose. Um aviso identifica poses sem ajuste automático.

Para aperfeiçoar manualmente o resultado, prima «Incorporar IK aos ajustes». O resultado atual é escrito numa nova animação de ajuste e IK desativada nessa pose e avatar. A animação original é conservada. Requer automação global e da pose ativas e definições IK aplicáveis.

Em Configuração do avatar, «Impedir flexão inversa dos joelhos» ativa a correção. Defina o ângulo nos detalhes «Flexão mínima do joelho (graus)». «Ativar correção específica do avatar» exige dados correspondentes. Algumas correções não são combináveis com IK dos membros.

## Poses originais, movimentos próprios e predefinições

Estas opções ficam em Configuração do avatar.

- «Usar poses originais em pé, agachado e de bruços»: importa automaticamente posturas do avatar.
- «Usar seus próprios movimentos»: abre importação separada por tipo, como em pé, agachado (Crouching) e de bruços (Prone). Regra geral, atribua um movimento por tipo.
- «Integrar bloqueios de rastreamento com outras ferramentas»: ative se os bloqueios falharem com outros sistemas.

A importação manual de movimentos próprios é separada da deteção automática das poses originais.

As predefinições estão no fim do separador. Pesquise o nome e use «Aplicar esta predefinição» para substituir as poses. Servem sobretudo na instalação inicial; guarde ajustes antes de substituir a configuração atual.

A lista carrega apenas nomes e informação dos avatares suportados. As animações de ajuste carregam ao aplicar a predefinição. Nem a criação inicial da lista antecipa as animações de todos os avatares.

A lista separa «Compatível com o avatar», «Predefinições do utilizador» e «Predefinições oficiais Unisaki Studio». Ocultar predefinições incluídas é uma preferência comum do projeto. Adicionar, editar e eliminar atualiza a lista. Use Recarregar se não mudar.

## Pré-cálculo e avisos

Com Usar poses originais ativo, os movimentos são detetados de Base em cada geração. Não são copiados para atribuições manuais: com apenas importação automática, a lista continua sem atribuições. Alterar Base ou ativar/desativar a importação aplica-se à geração seguinte.

Ao carregar, a importação automática afeta a cópia de compilação. Não modifica os objetos originais da cena, definições de importação, Controller fonte ou movimentos. Reutiliza o pré-cálculo quando as entradas não mudam.

Atribuições manuais têm prioridade por tipo, incluindo rotação, ajuste de animação e posição de reprodução. Desativar Usar ou retirar um movimento atribuído impede importação automática desse tipo. Limpar seleção manual remove definições e ajustes; com automação ativa, volta a detetar na geração seguinte. Undo reverte a operação. Entre vários registos antigos do mesmo tipo, prevalece o ativo selecionado no Inspector.

Não se eliminam automaticamente entradas criadas pela importação antiga, por serem indistinguíveis de registos manuais ou ajustados. Use Limpar seleção manual apenas nos tipos que deseja devolver à deteção automática. Com vários produtos, atribuições manuais noutros produtos também afetam as poses originais comuns.

O pré-cálculo acelera a compilação reutilizando dados gerados. Crie ou atualize em «Atualizar pré-cálculo». Uma compilação normal também gera o necessário; não é obrigatório pré-calcular manualmente cada vez.

Alterações exigem atualizar o pré-cálculo, com avisos em Verificar configurações no topo do Inspector e símbolos da Hierarchy. Atualize o pré-cálculo após mudar poses ou IK para reduzir o tempo de compilações futuras.

Configurações de exibição dos avisos está dentro de Verificar configurações. Pode ocultar tipos de aviso ou símbolos da Hierarchy. Ocultar não resolve problemas nem atualiza dados; erros e geração necessária continuam ativos.

## Notas de utilização

- Destina-se a rastreamento de 3 pontos e desktop. Fixar poses durante rastreamento de corpo inteiro não é suportado.
- Proporções e alterações podem exigir ajustes no contacto dos membros e na roupa.
- Camadas Base especiais ou outros sistemas de postura podem comportar-se de forma inesperada.
- Movimento/rotação do Modo foto e altura do chão corrigem poses da ferramenta e movimentos padrão ou próprios sem pose selecionada. Verifique substituições e vista no cliente VRChat.

## Termos e assistência

O código-fonte e os recursos deste repositório não são de código aberto. Exigem compra legítima e aceitação dos [termos incluídos](../../../%E3%82%86%E3%81%AB%E3%81%95%E3%81%8D%E3%83%9D%E3%83%BC%E3%82%BA%E3%82%B7%E3%82%B9%E3%83%86%E3%83%A0%E5%88%A9%E7%94%A8%E8%A6%8F%E7%B4%84%EF%BC%88%E6%97%A5%E6%9C%AC%E8%AA%9E%E7%89%88%EF%BC%89.pdf).

- [Manual de Kawaii Posing](https://docs.unisakistudio.jp/kawaiiposing/manual/)
- [Manual de Cool Posing](https://docs.unisakistudio.jp/coolposing/manual/)
- [Assistência](https://help.unisakistudio.jp/)

Ao comunicar problemas, indique versões do produto, Unisaki Posing System, Unity, VRChat SDK, Modular Avatar e NDMF, avatar, passos de reprodução e erros da Console.
