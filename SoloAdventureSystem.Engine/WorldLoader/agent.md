🧩 PROMPT — World Loader (Blazor + .NET)

Crie um módulo chamado WorldLoader para o projeto MUDVision, em Blazor Server (.NET 8).
Este módulo deve:

1. Função Principal

Ler um .zip enviado pelo usuário contendo todos os arquivos do mundo, descompactar em memória e carregar as estruturas internas do jogo.

2. Estrutura Esperada do .zip
/world.json
/rooms/*.json
/factions/*.json
/events/*.json
/npcs/*.json
/story/**/*.yaml
/assets/images/*.png   (opcional)
/assets/sounds/*.ogg   (opcional)

3. Especificações do Loader
3.1. Comportamento

Receber upload de um .zip

Descompactar (System.IO.Compression)

Validar arquivos obrigatórios:

world.json

Pelo menos 1 sala

Pelo menos 1 facção

Ler JSON/YAML

Popular modelos internos do MUDVision:

WorldModel

RoomModel

FactionModel

EventModel

NpcModel

StoryNode (YAML)

Armazenar o mundo carregado em um provedor singleton chamado WorldState.

3.2. Requisitos Técnicos

Projeto Blazor Server

C#

.NET 8

Nenhum pacote externo além de:

YamlDotNet para YAML

Tudo fortemente tipado

Métodos assíncronos (async/await)

Código limpo, documentado e enxuto

3.3. API Interna Necessária

Crie as seguintes classes e serviços:

Interfaces

IWorldLoader

IWorldState

Serviços

WorldLoaderService : IWorldLoader

WorldState : IWorldState

Métodos do Loader

Task<WorldModel> LoadFromZipAsync(Stream zipStream)

WorldModel ParseWorld(JsonDocument doc)

RoomModel ParseRoom(JsonDocument doc)

FactionModel ParseFaction(JsonDocument doc)

EventModel ParseEvent(JsonDocument doc)

NpcModel ParseNpc(JsonDocument doc)

StoryNode ParseStoryNode(string yamlContent)

Métodos do WorldState

void SetWorld(WorldModel world)

WorldModel? GetWorld()

bool IsLoaded { get; }

3.4. Validações

Arquivos malformados → exceção clara

Ausência de obrigatórios → exceção clara

YAML inválido → erro tratado

3.5. Segurança

Não escrever no disco (tudo em memória)

Sanitizar nomes de arquivos

Tamanho máximo 20 MB

4. Objetivo Final

Gerar todo o código necessário para:

Carregar mundos via .zip

Interpretar JSON e YAML

Preencher os modelos do jogo

Deixar o mundo acessível durante a sessão

Facilitar hot-reload de mundos sem reiniciar o servidor