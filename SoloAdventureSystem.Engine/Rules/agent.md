# 🎮 MUDVision — Prompt para gerar a RULE ENGINE baseada no DAEMON Simplificado

## 🎯 Objetivo
Gerar todo o código da **Rule Engine** do MUDVision em C# (.NET 9 ou 10), baseado no sistema DAEMON simplificado em inglês.  
A Rule Engine deve ser **completamente determinística**, sem dependência de IA, sem estado global e sem efeitos colaterais externos.

A Engine deve conter:
- modelos
- enums
- utilitários de rolagem
- verificações de ação
- combate
- testes sociais
- testes de medo
- testes de oposição
- vantagens/desvantagens
- cálculo de HP e defesa
- interpretadores básicos de regras

Nenhum código de UI deve ser criado.
Nenhum código de narrativa deve ser criado.
Nenhum serviço de mundo, loader ou SLM deve ser criado.

A entrega final é apenas o **núcleo mecânico do jogo**.

---

## 📦 Namespace
Use um único namespace: MudVision.Rules


---

# 📘 **REGRAS FORMALIZADAS (base para implementação)**

Copiar exatamente:

## Attributes (1–5)
- Soul
- Body
- Mind
- Presence

## Skills (0–3)
- Combat (Body)
- Stealth (Body)
- Knowledge (Mind)
- Awareness (Mind)
- Social (Presence)
- Will (Soul)
- Occult (Soul)

## Roll formula
`2d6 + Attribute + Skill`

## Target Numbers
- 7 Easy  
- 9 Standard  
- 11 Hard  
- 13 Extreme  

## Criticals
- 2 = Critical Failure  
- 12 = Critical Success  

## Advantage / Disadvantage
- Advantage → 3d6, keep highest 2  
- Disadvantage → 3d6, keep lowest 2  

## Combat
**Attack roll**  
`2d6 + Body + Combat` vs `Defense`

**Defense**  
`6 + Body + Combat` (or Awareness if Combat = 0)

**HP**  
`10 + Body * 2`

**Damage**  
- Light: 1d6  
- Medium: 1d6+2  
- Heavy: 2d6  
- Unarmed: max(1, 1d6-2)  

## Opposed Tests
Ambos rolam `2d6 + attr + skill`, maior vence.

## Fear / Morale
`2d6 + Soul/Presence + Will` vs TN

## Social Interactions
`2d6 + Presence + Social` vs `6 + Presence + Social`

## Supernatural Checks
`2d6 + Soul + Occult`

---

# 🧱 **CLASSES QUE O AGENTE DEVE GERAR**

### 1. Attribute enum
Enum contendo Soul, Body, Mind, Presence.

### 2. Skill enum
Enum para as skills listadas.

### 3. DamageType enum
Light, Medium, Heavy, Unarmed.

### 4. RollResult record
Contém:
- Total
- Dice
- Attribute
- Skill
- TargetNumber
- Success
- CriticalSuccess
- CriticalFailure

### 5. CharacterStats record
- Attributes: Dictionary<Attribute,int>
- Skills: Dictionary<Skill,int>
- HP
- Defense
- Methods apenas *de cálculo* podem existir como funções puras:
  - CalculateHP()
  - CalculateDefense()

### 6. DiceRoller static class
Funções:
- Roll2D6()
- Roll3D6()
- RollWithAdvantage()
- RollWithDisadvantage()

Deve retornar valores primitivos (int) ou listas.

### 7. RuleEngine class
Totalmente determinística, contendo métodos:

- **RollAction(attribute, skill, TN, advantage?)**
- **RollOpposed(attacker, defender, skillAttrPair)**  
- **RollCombatAttack(attackerStats, defenderStats)**
- **RollDamage(damageType)**
- **RollFearTest(stats, TN)**
- **RollSocialInteraction(attackerStats, defenderStats)**
- **RollSupernatural(stats, TN)**

Nenhum método deve usar random fora de DiceRoller.

### 8. AttackResult, DamageResult, OpposedResult records
Para padronizar todas as saídas.

### 9. AdvantageType enum
None, Advantage, Disadvantage.

---

# 👮‍♂️ Restrições Importantes
- Não incluir dependências externas.
- Não incluir logs, prints ou I/O.
- Não incluir async/await.
- Não incluir narrativa.
- Não incluir modificadores mágicos.
- Não incluir armaduras.
- Nenhuma decisão delegada ao SLM.

A Rule Engine deve ser 100% mecânica e segura.

---

# 🟢 Saída esperada pelo VS Agent
Gerar todos os arquivos .cs necessários contendo:

- enums  
- records  
- classes estáticas  
- RuleEngine com todos os métodos  
- Totalmente compilável  
- Sem warnings  
- Estilo claro e moderno (C# 12)  

Não gerar explicações — **somente código**.

