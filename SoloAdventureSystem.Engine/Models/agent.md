# 🎮 MUDVision — Prompt para gerar MODELOS do Mundo

## 📌 Objetivo
Gerar todas as classes de modelo usadas para carregar um mundo a partir de um `.zip`.  
Os modelos devem refletir exatamente a estrutura abaixo:

world/
  world.json
  locations/*.json
  npcs/*.json
  items/*.json
  factions/*.json
  story/*.yaml

O agente deve criar todas as classes, com propriedades tipadas e simples.  
Sem serviços, sem lógica, sem validação — apenas modelos puros.

---

## 🧱 Requisitos de Estilo
- Código em C# 12 / .NET 9  
- Usar `public record class` sempre que possível  
- Propriedades claras, consistentes e imutáveis quando fizer sentido  
- Sem métodos  
- Sem comentários desnecessários  
- Sem lógica adicional  
- Namespace único: `MudVision.World.Models`

---

## 📦 Modelos Necessários

### 1. WorldDefinition
Meta do mundo:
- Name
- Description
- Version
- Author
- CreatedAt
- StartLocationId
- Lists contendo IDs de: Locations, NPCs, Items, Factions, StoryNodes

### 2. Location
Representa uma sala:
- Id
- Name
- Description
- Connections: Dictionary<string, string> (direção → id destino)
- NpcIds: List<string>
- ItemIds: List<string>
- Tags: List<string>

### 3. NPC
- Id
- Name
- Description
- FactionId
- Hostility (enum simples)
- Attributes (Força, Destreza, Inteligência etc. — estilo DAEMON/GURPS Lite)
- Behavior (enum simples)
- Inventory: List<string>

### 4. Item
- Id
- Name
- Description
- Type (enum: Weapon, Consumable, Key, Misc)
- Weight
- Volume
- Damage / Bonus (inteiros opcionais)

### 5. Faction
- Id
- Name
- Description
- Ideology
- Relations: Dictionary<string, int> (outras facções → afinidade numérica)

### 6. StoryNode (via arquivo YAML)
- Id
- Title
- Text (texto longo via YAML `|`)
- Choices: lista contendo:
  - Label
  - NextNodeId
  - Effects (alterações de alinhamento, mover jogador, etc.)

### 7. Alignment
Eixos numéricos:
- OrderChaos
- EmpathyColdness
- SpiritMaterial

### 8. WorldPackage
Objeto que representa o mundo carregado:
- WorldDefinition
- List<Location>
- List<NPC>
- List<Item>
- List<Faction>
- List<StoryNode>

---

## 🎯 Comportamento do Agente
Quando rodar este prompt, o agente deve:

- Gerar **todas** as classes listadas  
- Usar o namespace `MudVision.World.Models`  
- Criar um arquivo por classe, ou tudo em um único arquivo (qualquer forma aceitável)  
- Não inventar campos não listados  
- Não criar métodos, validadores, serviços ou lógica  

A entrega final deve ser apenas o código C# dos modelos.
