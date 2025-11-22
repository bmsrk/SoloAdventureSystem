# ?? Solo Adventure System - Game Design Document (GDD)

**Version:** 2.0  
**Last Updated:** November 22, 2025  
**Status:** Active Development  
**Target Platform:** Web (Blazor WASM), Desktop (Terminal.GUI)

---

## ?? Executive Summary

**Genre:** Text-based Adventure RPG / Interactive Fiction  
**Core Loop:** Explore ? Interact ? Progress ? Discover  
**Unique Selling Points:**
- AI-generated procedural worlds
- Deterministic generation (reproducible from seeds)
- Rich narrative with branching stories
- Cyberpunk aesthetic
- Web and desktop support

**Target Audience:**
- Text adventure enthusiasts
- RPG players
- Procedural generation fans
- Developers/modders

---

## ?? Vision

Create an immersive, AI-powered text adventure platform where players explore richly detailed cyberpunk worlds, interact with deep NPCs, and shape their own stories through meaningful choices.

---

## ?? Core Gameplay

### Game Loop

```
1. EXPLORE
   ?
2. DISCOVER (locations, NPCs, items, secrets)
   ?
3. INTERACT (dialogue, combat, quests)
   ?
4. PROGRESS (level up, gain items, advance story)
   ?
5. CONSEQUENCES (world reacts to choices)
   ?
[Loop back to 1]
```

### Core Mechanics

#### 1. World Exploration
- **Movement:** Cardinal directions (N/S/E/W) + contextual (up/down)
- **Discovery:** Rooms, secrets, shortcuts, hidden areas
- **Mapping:** Mental map building, landmarks
- **Zones:** Different districts with unique atmospheres

#### 2. NPC Interaction
- **Dialogue:** Branching conversations with choices
- **Relationships:** Build trust, make enemies, form alliances
- **Quests:** NPCs provide missions and story
- **Schedules:** NPCs move and have routines (future)

#### 3. Combat System (Phase 2)
- **Turn-Based:** Strategic combat
- **Dice Rolls:** d20 system for skill checks
- **Attributes:** STR, DEX, INT, CON, WIS, CHA affect outcomes
- **Tactics:** Positioning, cover, abilities

#### 4. Character Progression
- **Leveling:** Gain XP from combat, quests, exploration
- **Stats:** Allocate points to attributes
- **Skills:** Unlock new abilities
- **Classes:** Choose playstyle (Phase 4)

#### 5. Quest System (Phase 2)
- **Main Quest:** Drive the narrative forward
- **Side Quests:** Optional content, character development
- **Hidden Quests:** Discovered through exploration
- **Quest Chains:** Multi-stage stories
- **Consequences:** Choices affect outcomes

#### 6. Inventory & Items (Phase 2)
- **Equipment:** Weapons, armor, tools
- **Consumables:** Health packs, buffs
- **Quest Items:** Key items for progression
- **Trading:** Buy/sell with NPCs
- **Crafting:** Combine items (Phase 4)

---

## ?? World Design

### Current World Structure (MVP 1.0)

```
World
??? Zones (10-15 areas)
?   ??? Rooms (5-10 per zone)
?   ?   ??? Description
?   ?   ??? Exits (connections)
?   ?   ??? NPCs (0-3)
?   ??? Theme (corporate, slums, industrial)
??? Factions (1-3)
?   ??? Members (NPCs)
?   ??? Territory
??? Story Nodes (basic)
```

### Enhanced World Structure (Phase A - Target)

```
World
??? Zones (Districts)
?   ??? Type (Corporate, Industrial, Residential, Underground, etc.)
?   ??? Security Level (Public, Restricted, High-Security)
?   ??? Atmosphere
?   ?   ??? Lighting (neon, dim, harsh)
?   ?   ??? Sounds (ambient)
?   ?   ??? Smells (atmosphere)
?   ?   ??? Temperature
?   ??? Rooms
?       ??? Type (Plaza, Corridor, Vault, Market, etc.)
?       ??? Rich Description (multi-sensory)
?       ??? Points of Interest
?       ?   ??? Interactive Objects
?       ?   ??? Lore Items
?       ?   ??? Secrets
?       ??? Connections (smart routing)
?       ??? Dynamic Elements (state changes)
?
??? NPCs
?   ??? Personality
?   ?   ??? Backstory (2-3 paragraphs)
?   ?   ??? Motivation (what drives them)
?   ?   ??? Traits (friendly, suspicious, etc.)
?   ?   ??? Secrets (hidden information)
?   ?   ??? Interests
?   ??? Relationships
?   ?   ??? Allies
?   ?   ??? Enemies
?   ?   ??? History
?   ??? Dialogue Trees
?   ?   ??? Greetings (based on relationship)
?   ?   ??? Information (rumors, lore)
?   ?   ??? Quests (offer missions)
?   ?   ??? Trading (future)
?   ??? Schedule (future)
?
??? Factions
?   ??? Goals & Ideology
?   ??? Territory & Influence
?   ??? Members (NPCs)
?   ??? Relationships (allied/hostile)
?   ??? Resources
?
??? Story System
    ??? Main Quest Line
    ?   ??? Acts (3-5)
    ?   ??? Key Events
    ?   ??? Climax & Resolution
    ??? Side Quests
    ?   ??? Character-driven
    ?   ??? Zone-specific
    ?   ??? Hidden quests
    ??? Lore Distribution
    ?   ??? Terminal Logs
    ?   ??? Documents
    ?   ??? NPC Dialogue
    ?   ??? Environmental Details
    ??? Consequences
        ??? Choice Tracking
        ??? World State Changes
        ??? Branching Outcomes
```

### Zone Types

| Zone Type | Description | Security | NPCs | Atmosphere |
|-----------|-------------|----------|------|------------|
| **Corporate** | High-rise offices, executive areas | High | Execs, guards, clerks | Clean, cold, monitored |
| **Industrial** | Factories, warehouses, production | Medium | Workers, foremen | Loud, dirty, dangerous |
| **Residential** | Living quarters, apartments | Low-Medium | Residents, families | Varied by district |
| **Commercial** | Markets, shops, entertainment | Low | Merchants, customers | Busy, crowded, colorful |
| **Underground** | Criminal, hidden, black market | Variable | Criminals, outcasts | Dark, secretive, tense |
| **Transit** | Connections, stations, hubs | Medium | Travelers, security | Transient, echoing |
| **Wilderness** | Abandoned, outskirts, ruins | None | Scavengers, mutants | Desolate, eerie, forgotten |

---

## ?? Character Design

### Player Character

**Starting Stats:**
```
HP: 100/100
Level: 1
XP: 0/100

Attributes (10 base):
- STR (Strength): 10
- DEX (Dexterity): 10
- INT (Intelligence): 10
- CON (Constitution): 10
- WIS (Wisdom): 10
- CHA (Charisma): 10
```

**Progression:**
- Level up: 100 XP per level (scaling)
- Stat points: 3 per level
- HP increase: CON modifier + 10 per level
- Skills unlock at specific levels

**Future: Character Classes (Phase 4)**
- Netrunner (Hacker) - INT/DEX focused
- Street Samurai (Combat) - STR/DEX focused
- Face (Social) - CHA/WIS focused
- Tech (Engineer) - INT/WIS focused

### NPC Design Philosophy

**Depth over Quantity:**
- Better to have 20 memorable NPCs than 100 generic ones
- Each NPC should serve a purpose (quest, lore, atmosphere)
- Relationships between NPCs create depth

**NPC Tiers:**

| Tier | Role | Detail Level | Example |
|------|------|--------------|---------|
| **Major** | Quest givers, story critical | Full backstory, dialogue trees, relationships | Marcus Chen (Corp Exec) |
| **Supporting** | Zone flavor, side quests | Moderate backstory, limited dialogue | Sarah (Bartender) |
| **Minor** | Atmosphere, info dumps | Basic info, 1-2 dialogue lines | Generic Guard |

---

## ?? Narrative Design

### Story Structure

**Three-Act Structure:**

```
ACT 1: Setup & Hook
- Player arrives in world
- Establish setting and tone
- Introduce main conflict
- Meet key NPCs
- Initial quest hook
?
ACT 2: Escalation
- Explore different zones
- Build relationships
- Uncover conspiracy/threat
- Choice points with consequences
- Rising tension
?
ACT 3: Resolution
- Confront main threat
- Consequences of earlier choices
- Multiple possible endings
- Epilogue showing world state
```

### Branching Narrative

**Choice Categories:**
1. **Factional:** Align with factions (consequences)
2. **Moral:** Good/evil choices (reputation)
3. **Tactical:** How to approach situations (outcomes)
4. **Personal:** Character development (relationships)

**Example Branch:**
```
Quest: Corporate Data Heist
?? Choice 1: How to infiltrate?
?  ?? Stealth (DEX check)
?  ?? Social Engineering (CHA check)
?  ?? Bribe Guard (requires credits)
?
?? Choice 2: What to do with data?
?  ?? Sell to highest bidder (credits, no faction)
?  ?? Give to rebels (rebel faction +, corp faction -)
?  ?? Destroy it (neutral, moral choice)
?  ?? Keep it (blackmail option later)
?
?? Consequences:
   ?? Faction reputation changes
   ?? NPC reactions change
   ?? New quests unlock/lock
   ?? World state affected
```

### Lore Integration

**World Building Elements:**
- Corporate wars and history
- Technology advancement timeline
- Faction origins and conflicts
- Key historical events
- Current political climate
- Underground culture

**Delivery Methods:**
- Terminal logs (hacking)
- NPC dialogue (stories)
- Environmental storytelling (visual details)
- Found documents (lore dumps)
- News broadcasts (current events)
- Graffiti (cultural commentary)

---

## ?? Aesthetic & Tone

### Visual Style (Text-Based)

**Cyberpunk Noir:**
- High-tech, low-life
- Neon-drenched nights
- Corporate dystopia
- Urban decay
- Technological saturation

**Color Palette (Terminal UI):**
- Primary: Cyan (#00FFFF)
- Secondary: Magenta (#FF00FF)
- Accent: Neon Green (#00FF00)
- Warning: Orange (#FF8800)
- Danger: Red (#FF0000)
- Text: White/Light Gray

**Descriptive Language:**
- Vivid sensory details
- Technical jargon (believable)
- Street slang (flavor)
- Corporate speak (satire)

### Tone

**Primary:** Atmospheric, immersive, mysterious  
**Secondary:** Dark humor, satirical, gritty  
**Avoid:** Overly edgy, grimdark, hopeless

**Example Descriptions:**

? **Generic:**
> "You are in a room. There is a guard here."

? **Atmospheric:**
> "The corporate plaza stretches before you, bathed in the cold glow of holographic advertisements. A security guard leans against a chrome pillar, his neural implant blinking red as he scans the crowd with augmented eyes."

---

## ?? Development Roadmap

### Phase A: Enhanced World Generation (Current) - 3 weeks

**Goals:**
- Rich, atmospheric room descriptions
- Deep NPC personalities with dialogue
- Zone-based world structure
- Connected story and quests
- Environmental storytelling

**Success Criteria:**
- Worlds feel alive and unique
- NPCs memorable and distinct
- Story coherent and engaging
- High replayability

### Phase 2: Core Gameplay (v1.1) - 4-6 weeks

**Features:**
- Turn-based combat system
- Inventory and item management
- Quest tracking and journal
- Save/load functionality

**Success Criteria:**
- Combat is fun and strategic
- Items feel meaningful
- Quests drive exploration
- Can save progress

### Phase 3: Polish & Enhancement (v1.2) - 2-3 weeks

**Features:**
- Improved UI/UX
- Minimap and navigation aids
- Status effects and buffs
- Configuration and settings

**Success Criteria:**
- Smooth user experience
- Clear feedback systems
- Customizable gameplay

### Phase 4: Advanced Features (v2.0) - 6-8 weeks

**Features:**
- Character classes and builds
- Skill trees and abilities
- Advanced game mechanics (stealth, hacking, crafting)
- Dynamic world events

**Success Criteria:**
- Deep character customization
- Multiple playstyles viable
- World feels reactive

### Phase 5: Platform Migration - 4-6 weeks

**Features:**
- Blazor WASM web interface
- Mobile-responsive design
- Cloud deployment (containers)
- Offline capability (PWA)

**Success Criteria:**
- Web-accessible
- Mobile-friendly
- Fast loading
- Production-ready hosting

### Phase 6: Social & Content (v3.0) - Ongoing

**Features:**
- World sharing platform
- Community content
- Leaderboards and achievements
- Multiplayer (experimental)

**Success Criteria:**
- Active community
- User-generated content
- Social features working

---

## ?? Testing Strategy

### Unit Tests
- World generation algorithms
- Combat calculations
- Dialogue tree navigation
- Quest state management
- Inventory operations

### Integration Tests
- Full world generation
- NPC interaction flows
- Quest completion paths
- Save/load functionality

### Playtesting Focus
- Is world exploration fun?
- Are NPCs engaging?
- Is combat satisfying?
- Are quests motivating?
- Is progression rewarding?

### Performance Targets
- World generation: < 30 seconds
- World loading: < 2 seconds
- Command response: < 100ms
- Memory usage: < 500MB

---

## ?? Success Metrics

### Player Engagement
- Average session length: > 30 minutes
- Return rate: > 60%
- Completion rate: > 40%

### Content Quality
- NPC memorability: Player can name 5+ NPCs
- World coherence: Story makes sense
- Replayability: Different seeds feel unique

### Technical Quality
- Bug rate: < 1 critical per 100 sessions
- Crash rate: < 0.1%
- Load time: < 3 seconds
- Test coverage: > 80%

---

## ?? Future Possibilities

### Potential Features (Post-v3.0)
- Voice narration (AI-generated)
- Procedural music generation
- Image generation for key scenes
- VR text adventure mode
- Modding support and SDK
- Steam release
- Mobile native apps
- Console ports (text-based)

### Monetization Options (If applicable)
- Premium world packs
- DLC expansions
- Cosmetic customization
- Support via Patreon/Ko-fi
- Steam Workshop integration

---

## ?? Technical Specifications

### Architecture
- **Engine:** .NET 10 class library
- **Frontend:** Blazor WASM (future), Terminal.GUI (current)
- **AI:** Multiple providers (GROQ, OpenAI, Azure, GitHub)
- **Storage:** ZIP world packages, JSON/YAML data
- **Deployment:** Docker containers, Azure Static Web Apps

### Data Models

**World Package Structure:**
```
world.zip
??? world.json              # Metadata
??? rooms/*.json            # Room definitions
??? npcs/*.json             # NPC data
??? factions/*.json         # Faction info
??? quests/*.json           # Quest definitions (future)
??? items/*.json            # Item database (future)
??? story/*.yaml            # Story nodes
??? system/                 # Generation metadata
```

### Performance Requirements
- World generation: Deterministic, < 30s
- World loading: < 2s
- Command processing: < 100ms
- Memory footprint: < 500MB
- Concurrent players: 1 (single-player focus)

---

## ?? Team & Roles

### Current Team
- **Developer/Designer:** Bruno (You)
- **AI Assistant:** GitHub Copilot (Me)

### Future Roles (If scaling)
- Content Writer (quests, dialogue)
- World Designer (zone layouts)
- Balance Designer (combat, progression)
- Community Manager
- QA Tester

---

## ?? Version History

### v1.0 - MVP Complete (Nov 2025)
- ? Basic world generation
- ? Terminal.GUI interface
- ? Room navigation
- ? NPC interactions
- ? 5 AI providers
- ? 30 passing tests
- ? Complete documentation

### v2.0 - Enhanced Generation (Target: Dec 2025)
- [ ] Rich world generation
- [ ] Deep NPC personalities
- [ ] Connected story system
- [ ] Zone-based structure

### v1.1 - Core Gameplay (Target: Jan 2026)
- [ ] Combat system
- [ ] Inventory management
- [ ] Quest tracking
- [ ] Save/load

---

## ?? Vision Statement

**"Create immersive, AI-powered adventures that feel hand-crafted, not generated."**

Every world should tell a story. Every NPC should have purpose. Every choice should matter. Use AI as a tool for creativity, not a replacement for design.

---

## ?? References & Inspiration

### Games
- Zork (text adventure pioneer)
- Disco Elysium (deep dialogue)
- Cyberpunk 2077 (aesthetic)
- AI Dungeon (AI generation)
- Dwarf Fortress (procedural depth)

### Tech
- GPT models for text generation
- Deterministic algorithms
- Graph-based world structures
- Branching narrative systems

---

**Last Updated:** November 22, 2025  
**Next Review:** December 2025 (after Phase A)  
**Status:** Living Document - Update as design evolves

---

This GDD serves as the north star for development. Refer back when making design decisions!

?? **Let's make something amazing!** ?
