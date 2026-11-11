# Histórico de alterações

## [3.2.0] - Não lançado

### Adicionado

- Inspector, comandos de edição, diálogos de instalação, nomes de poses padrão e menus VRChat gerados em 30 idiomas.
- Idioma unificado entre Inspector e menu no jogo. Inicialmente utiliza o idioma do computador e compartilha a última escolha com outros projetos do mesmo usuário.
- README, histórico e manual offline abertos no idioma escolhido.
- Nomes das 128 poses padrão e descrições de todas as funções alinhados com a terminologia existente.

### Compatibilidade

- Mantidas configurações existentes, nomes próprios, referências a recursos, identificadores de parâmetros e armazenamento da licença.

## [3.1.4] - 2026-11-11

- Corrigido o problema em que as animações específicas do avatar atribuídas a agachar e deitar de bruços não eram aplicadas, sendo usadas as poses padrão do VRChat. A correção abrange a importação automática e as atribuições manuais.
- Corrigidos os cabelos, óculos e outros elementos conectados com MA Bone Proxy que permaneciam na posição original durante a edição ou prévia de poses. Agora acompanham a pose da prévia, preservando as configurações e o posicionamento do avatar original.
- Corrigida a falha da correção de pose baseada na posição do ponto de vista ao capturar imagens de poses como «仰向け寝_お疲れ». As imagens usam uma animação que inclui as correções de pose e do chão aplicadas durante a compilação.

No ALCOM / VCC, atualize Unisaki Posing System para 3.1.4. Você pode continuar usando Kawaii Posing 3.1.2 e Cool Posing 1.1.0 sem alterações. Se usava pré-cálculo, pressione «Atualizar pré-cálculo» após a atualização, salve a cena e compile e envie o avatar novamente.

## [3.1.3] - 2026-11-09

- «Atualizar miniaturas das poses» e o pré-cálculo em um avatar duplicado não excluem mais as imagens de poses do avatar original.
- Corrigida a indicação «Configurações alteradas» após apenas capturar as imagens novamente, quando o pré-cálculo tinha sido executado antes do primeiro salvamento da cena ou no modo de edição de Prefab.
- Corrigido o problema em que o pré-cálculo podia parar no meio se restassem dados de poses de um pré-cálculo anterior.
- A abertura da lista ficou mais leve quando ainda não existem imagens de poses. Também foi revisada a espera que bloqueava o Unity até terminar a obtenção de informações dos pacotes no início do pré-cálculo ou da compilação.
- As configurações de edição das ferramentas de poses desativadas dentro do avatar também são removidas nas compilações para PC e Android.
- «Pedir ajuda sobre este erro» pode incluir no rascunho números de diagnóstico e outras informações mais adiante na Console, mesmo quando os primeiros erros não permitem identificar a causa. Se ocorrer um erro nas configurações de clipes de «Virar», o rascunho também inclui os pontos a verificar.

No ALCOM / VCC, atualize Unisaki Posing System para 3.1.3. Você pode continuar usando Kawaii Posing 3.1.2 e Cool Posing 1.1.0 sem alterações. Como esses produtos indicam uma versão anterior do sistema comum, pode aparecer uma confirmação de conflito durante a atualização. Se usava pré-cálculo, pressione «Atualizar pré-cálculo» após a atualização, salve a cena e compile e envie o avatar novamente.

## [3.1.2] - 2026-11-09

- Corrigido o rosto que continuava para cima após levantar da posição de bruços com a redução de deformação por cabeça ativa.
- Fixação da cabeça disponível em «Configurações → Fixação automática da cabeça de costas» no menu EX mesmo sem Altura pés. Entradas desaparecidas em 3.1.0 ou 3.1.1 voltam após atualização e compilação.
- Atualizar pré-cálculo funciona também com o objeto principal do avatar desativado.
- Corrigida falha ocasional de geração após atualizar se restava um Controller comum antigo em Assets. Alterações pessoais do Controller são mantidas.
- Se a ferramenta ou um ascendente desativado impedir o Animator comum, o nome é mostrado e Verificar configurações permite selecionar o objeto.
- Nos detalhes do pré-cálculo pode consultar os elementos diferentes do último registro quando houver alterações.
- Ao parar por erro de script (PS050), mostra até 5 erros originais do Unity. A consulta ao suporte abre um rascunho verificável/editável com versões do Unity e ferramentas e número do erro, que pode enviar.

Se usava pré-cálculo, atualize-o após a instalação, salve a cena e compile e envie novamente o avatar.

## [3.1.1] - 2026-11-08

- Altura chão e Virar ativados aparecem nas Configurações do menu EX mesmo com Espelhar desligado.
- Movimentos corretos de Virar conservados para Pernas esticadas, Relaxado e Um joelho levantado após ordenação, eliminação ou renomeação antiga, incluindo Unpack completo se a origem for identificável. Mantidas poses próprias, Virar desativado intencionalmente e animações laterais substituídas.
- Corrigidas posição de bruços breve ao selecionar avatar e posição de costas breve ao saltar com apenas bruços selecionados. O salto é desativável em Funções → Funções práticas → Trocar poses deitadas pulando, ativo inicialmente.
- Referências recuperáveis perdidas do Animator Controller comum geram aviso e reconstrução pelas configurações. Referências compartilhadas entre avatares mantêm-se.
- Erros de compilação param o processo logo no início, indicando a causa. A limpeza automática espera enquanto os scripts impedem verificar referências.
- Prefab oficial preferido mesmo havendo outro com o mesmo nome; diferenças só de fim de linha no Controller não impedem pré-cálculo.
- Write Defaults dos níveis FX adicionados, como Altura chão, seguem o avatar.

Se usava pré-cálculo, atualize-o na ferramenta da cena após instalar, salve e compile/envie novamente o avatar.

## [3.1.0] - 2026-11-07

- Ajuste automático IK conforme proporções em poses compatíveis, correções específicas e prevenção da flexão inversa dos joelhos.
- Modo foto com posição/direção e sincronização e cabeça visível opcionais.
- Altura chão move todo o corpo até 3m acima/abaixo, separadamente dos pés.
- Respiração, supressão do balanço padrão e correção de deformação pela direção do pescoço.
- Funções e consumo no Inspector, pesquisa, vários clips, ordem e prévia na Scene.
- Mudança desktop em pé/agachado/sentado acelerada.
- Avisos de pré-cálculo, deslocação no Inspector e pausas de limpeza melhorados.
- Corrigidos aviso falso e regeneração ao enviar com importação automática e pré-cálculo inalterado. Resultados aplicados ao avatar gerado sem mudar a importação original.
- Virar pela inclinação da cabeça e movimentos compatíveis por pose.
- Melhorados transferência de predefinições antigas, movimentos próprios e recursos gerados.

## [3.1.0-beta.2] - Beta aberto (2026-11-05)

- Comprimento fixo e diferenças de altura (Y) e profundidade (Z) entre Head e Viewpoint como referências IK.
- Avisos também à esquerda dos nomes na Hierarchy: pré-cálculo ausente/alterado e avisos de importação ou predefinições.
- Corrigidas pausas longas ao abrir cenas quando a limpeza lia Controllers gerados de outras cenas.
- Corrigida perda de histórico ao mudar poses ou funções. Agora avisa alterações posteriores ao pré-cálculo, em vez de execução ausente, também na Hierarchy.
- Comparações compartilhadas entre Inspector/Hierarchy para reduzir deslocação pesada sem alterações. A compilação continua a comparar sempre.

## [3.1.0-beta.1] - Beta aberto (2026-11-04)

- Mudança normal desktop com uma avaliação para compilação/pré-cálculo; VR e exceções mantêm o percurso anterior.
- Pré-cálculos alpha.3 ou anteriores reconhecidos como por atualizar.
- Vista conservada ao mudar Altura chão no Modo foto e restaurada à última altura no fim.

- Importação automática apenas detecta na geração, sem adicionar/alterar atribuições manuais na compilação ou pré-cálculo.
- Regeneração acompanha ativação automática, alterações de Base e remoção manual, preservando atribuições compartilhadas e restaurando movimentos padrão em vez dos antigos importados.
- Controllers em um arquivo por avatar e GUID de cena; referências compartilhadas por duplicação, outra cena ou Prefab separadas antes de atualizar.
- Recursos temporários NDMF usados no build sem aumentar arquivos de edição gerados.
- Verificação ao abrir/salvar cenas: protege edições não salvas, Undo/Redo e referências externas; só arquivos sem uso de propriedade certa vão para a lixeira.
- Rotação, animação de ajuste e posição de reprodução aplicadas à importação; rotação, RootT/RootQ e Humanoid unificados com poses normais.
- Ajustes BlendTree aninhados duplicados para o destino, mantendo movimento fonte e mistura de caminhada.
- Reprodução só em pé convertida em pose estática à frequência original, sem parar a caminhada.

## [3.1.0-alpha.2] - Teste fechado (2026-10-26)

### Correções e alterações

- Modo foto e Altura chão aplicados também a movimentos padrão/próprios sem seleção.
- Altura chão ampliada a 3m acima/abaixo: 50% original e inicial, 0% 3m abaixo, 100% 3m acima.
- Corrigida alteração do BlendTree fonte ao substituir só em pé.
- Correção manual em poses sem IK, sem avisos de geração desnecessários.
- Corrigidas misturas de rascunhos homónimos e mudanças de movimento ignoradas no mesmo recurso.
- Corrigido número de pose inicial desativada que selecionava outra.
- Explicação em vez de exceção perante AnimatorOverrideController não suportado.
- Virar desativado explicitamente mantido nos predefinições antigas.
- Ícones mais legíveis no tema claro.

Continuam as verificações da vista e de movimentos padrão substituídos no cliente VRChat.

## [3.1.0-alpha.1] - Antes de publicar

### Adicionado

- Ativação por função e consumo de ExpressionParameters.
- Modo foto com avanço e rotação do avatar.
- Sincronização opcional: 0 bit local, 16 bit sincronizados.
- Cabeça própria visível no Modo foto opcional.
- Altura chão move corpo e cabeça 3m acima/abaixo: 50% original/inicial, 0% 3m abaixo, 100% 3m acima.
- Virar por cabeça e animações por pose.
- Respiração Fraco, Médio, Forte, Rápido e Personalizado.
- Movimento e menu de respiração separados: 0 bit movimento, 1 bit ativação sincronizada.
- Supressão do balanço normal do VRChat.
- Correção de deformação por direção do pescoço.
- IK automática conforme corpo nas poses compatíveis, global ou por pose.
- Incorporar IK aos ajustes salva resultado em animação.
- Correção específica, prevenção de flexão inversa e ângulo mínimo dos joelhos.
- Prévia simples na Scene.
- Separadores por categoria, pesquisa transversal, vários clips e ordem de destino.
- Histórico do produto e sistema no Inspector.

### Alterações

- Predefinições mais rápidas ao adicionar ferramenta e abrir lista.
- Lista atualizada após adicionar, editar ou eliminar.
- Inspector em 3 separadores: Funções, Poses, Configuração do avatar.
- Descrições, ícones e abertura de detalhes uniformes.
- Respiração nas funções e predefinições no fim da configuração.
- Movimentos próprios separados da importação automática original.
- Menos animações duplicadas para Altura pés.
- Atualização de imagens com botão e confirmação.
- Avisos reunidos em Verificar configurações.
- Novos Prefabs 8bit retirados em favor do normal configurável, mantendo referências.
- SDK mínimo 3.10.4.

### Correções

- Modo foto e chão também corrigem movimentos sem pose selecionada.
- BlendTree fonte preservado ao substituir só em pé.
- Ajustes manuais sem IK válida e avisos IK desnecessários suprimidos.
- Corrigidas misturas entre poses homónimas e movimentos substituídos no mesmo recurso não regenerados.
- Número de pose inicial desativada reutilizado não provoca seleção errada.
- Aviso em vez de exceção com AnimatorOverrideController não suportado na importação original.
- Virar explicitamente desligado mantido nas predefinições antigas.
- Fundo dos ícones melhorado para tema claro.

- Ícones de Virar, Altura chão e Modo foto ausentes ao adicionar corrigidos.
- Perda de Virar com predefinições antigas corrigida.
- Edição de nome já não passa para outra pose ao abrir detalhes.
- Detalhes fechados ao ordenar e Undo/Redo corrigidos.
- Avatar original sobreposto e posição adicionada duas vezes na prévia corrigidos.
- Erros de prévia com componentes ausentes corrigidos.
- Diferenças IK de orientação/posição por eixos e escalas dos ossos corrigidas.
- Avisos e pré-cálculo atualizados após alterações.
- Pré-cálculo antigo não reutilizado após alterações IK.
- Erros de compilação de funções adicionais em certas configurações corrigidos.

## [3.0.16] - 2026-09-14
- NDMF mínimo atualizado a 1.14.0 devido à API `VirtualAnimatorController.SetParameter`.

## [3.0.15] - 2026-09-13
- Corrigidas grandes alterações de posição e joelhos ao alternar fixação do pescoço de costas em avatares com escala Hips alterada e FloorAdjuster.

## [3.0.14] - 2026-09-09
- Corrigida ausência de Sem gestos a partir da segunda ferramenta no mesmo avatar.
- Corrigido regresso de USSPS_Locomotion a Standing apesar de poses como SleepDown.
- Altura e X/Z com FloorAdjuster corrigidos, incluindo ordem das versões MA e AAO.
- PosingSystem_Locomotion padrão restaurado automaticamente se o AnimatorController de Generated for eliminado.
- Erro ao eliminar todas as poses em pé corrigido.
- Predefinições ordenadas por nome do avatar.

## [3.0.13] - 2026-09-04
- Corrigida autotransição em pé que tornava instável o regresso logo após começar a agachar em VR.

## [3.0.12] - 2026-09-04
- Corrigida falsa detecção de alterações ao compilar sem pré-cálculo com NDMF.
- `AddAssetToSameFile` ao adicionar recursos gerados a AnimatorController temporário NDMF corrigido.

## [3.0.11] - 2026-08-31
- Falha por Scene inválida do avatar de pré-cálculo com NDMF 1.14.7 ou posterior corrigida.
- `MissingReferenceException` ao mostrar erro com LocalizationAsset destruído corrigida.

## [3.0.10] - 2026-08-29
- Recursos reais de predefinições carregados apenas ao abrir seleção e cache reutilizada se o índice GUID não mudar.
- Ícones Hierarchy reavaliados apenas em alterações e pesquisa de menus em cache comum. Reduzidas pausas periódicas em cenas com muitas ferramentas.
- `ArgumentException` por parâmetros duplicados no pré-cálculo de várias ferramentas no mesmo avatar corrigida.

## [3.0.9] - 2026-08-26
- NullReferenceException evitada se não se obtiver VRCAvatarDescriptor de PosingSystem, indicando os registros necessários.
- `Avatar is null` por reutilizar prévia de Avatar temporário criado/eliminado por NDMF em builds ou pré-cálculos posteriores corrigido. Conversão separada da cache e colisões de nomes temporários entre prévias evitadas.
- Referência da prévia 「可愛いポーズ」 não salva e acumulação de avatares ocultos corrigidas. Prévias excluídas da gravação da cena e antigas recuperadas ao abrir cena, entrar em Play ou recarregar scripts.

## [3.0.8] - 2026-08-18
- Unity Assertion e risco de dados inválidos em nova correção de altura por FloorAdjuster e semelhantes corrigidos.
- Medição protegida contra interferência no avatar original e AnimationMode de outras ferramentas.

## [3.0.7] - 2026-08-12
- Vista deslocada e joelhos anormais de costas com ferramentas de altura, como FloorAdjuster, corrigidos.
- Erro de PosingSystem fora do avatar corrigido (obrigado a PancakeTorto1se).
- Continuação após falha de envio e erros resultantes corrigidos.
- Falsas alterações logo após pré-cálculo corrigidas.
- Importação automática de locomoção melhorada.
- Avisos para certos rigs especiais adicionados.
- Confirmação e espelhamento de valores na janela de ajuste adicionados (obrigado a kraihd).
- Orientações para erros de outras ferramentas e verificações dos pontos problemáticos adicionadas.

## [3.0.6] - 2026-03-17
- Poses deformadas em alguns avatares com controle próprio corrigidas.

## [3.0.5] - 2026-01-31
- Encerramento inesperado do Unity com certos ExpressionMenus corrigido.
- Problemas do Prefab de integração ごろ寝EX corrigidos (obrigado a あずき).
- Rotação errada na fusão de animações de ajuste corrigida.

## [3.0.4] - 2026-01-11
- Comportamento de avatares em Prefabs parcialmente melhorado.
- Remoção de licença adicionada.
- Problemas do pescoço em muitas poses corrigidos.
- Abertura do Inspector ligeiramente aliviada.
- Ícones gerados também no pré-cálculo e texto de atualização alterado.
- Avisos ocultos sem pré-cálculo anterior, visto muitos usuários não parecerem usá-lo.
- Recursos desnecessários removidos.
- Dedos impossíveis de ajustar corrigidos.

## [3.0.3] - 2026-01-04
- Aplicação de predefinição diferente da escolhida corrigida (obrigado a あずき).

## [3.0.2] - 2026-01-02
- Tema escuro suportado.
- Poses deformadas após rotação no ajuste corrigidas.
- Ícones redimensionados/comprimidos e opção ainda menor adicionada.
- Integração de rastreamento e importação automática retiradas em 3.0.0 restauradas.

## [3.0.1] - 2025-12-29
- Configuração de movimentos padrão corrigida.
- Posturas que se deformavam sozinhas corrigidas.

## [3.0.0] - 2025-12-28
- Pré-cálculo adicionado.
- Ajuste de animação adicionado.
- Predefinições adicionadas.

## [2.2.5] - 2025-05-08
- Compatibilidade com certos sistemas corrigida.

## [2.2.4] - 2025-04-04
- Poses que não mudavam com certos avatares e ModularAvatar corrigidas.

## [2.2.3] - 2025-03-13
- Movimentos estranhos de avatares, como salto de 森羅, corrigidos.
- Valor inicial de Sem gestos incorreto em certos casos corrigido.

## [2.2.2] - 2025-03-06
- Erros em certos ambientes corrigidos.

## [2.2.1] - 2025-02-05
- Falha do Unity corrigida.
- Versão no nome da ferramenta em NDMF Console adicionada.

## [2.2.0] - 2025-02-02
- Sem gestos para usar mãos da animação adicionado.

## [2.1.6] - 2025-02-02
- Poses deformadas com certas ferramentas corrigidas.
- Erros dependentes das opções corrigidos.

## [2.1.5] - 2024-12-10
- Conflitos com certos sistemas resolvidos.
- Botão para remover Unitypackage adicionado.

## [2.1.4] - 2024-12-08
- Erros em certos avatares corrigidos.
- Fixação dos pés inutilizável por conflitos corrigida.
- Posturas a regressar ao padrão VRC no desktop corrigidas.

## [2.1.3] - 2024-12-01
- Mudança impossível com VRMode de tipo errado em avatares/sistemas corrigida.
- Conflitos de tipos de parâmetros mostrados em NDMF-Console.

## [2.1.2] - 2024-12-01
- Explicação em NDMF-Console quando erros externos causam erro no sistema durante compilação.
- Falhas de compilação em certos avatares corrigidas.
- Comportamento após carregar avatar melhorado.
- Processamento original de locomoção removido por impedir funcionamento em certos avatares.

## [2.1.1] - 2024-11-20
- Problemas MergeAnimator de PosingOverride corrigidos.

## [2.1.0] - 2024-11-20
- Pés/quadril enterrados no desktop corrigidos para todas as poses.
- Mudança independente da altura da cabeça adicionada.
- Modo sem ícones para Quest adicionado.
- Avisos de problemas no avatar adicionados.
- Conflitos com AFK e bloqueios de braços de outras ferramentas resolvidos.
- Fixação automática do pescoço de costas adicionada.

## [2.0.0] - 2024-11-12
- Versão VPM/VCC lançada.
- Nome da ferramenta em erros alterado #1.
- Termos adicionados #5.
