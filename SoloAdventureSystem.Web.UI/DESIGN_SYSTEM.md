# SoloAdventureSystem Design System

## Overview
This design system provides a consistent, themeable UI foundation built with CSS variables. It eliminates Shadow DOM issues from FluentUI by using custom Blazor components with regular HTML/CSS.

## Theme Structure

### CSS Variables
All theme values are defined using CSS custom properties in `theme.css`:

```css
:root {
    /* Colors */
    --color-bg-primary: #0a0a0a;
    --color-text-primary: #ffffff;
    --color-accent-primary: #3b82f6;
    /* ... and more */
}
```

### Switching Themes
To switch themes, simply change the CSS variable values or add a `data-theme` attribute:

```html
<body data-theme="light">
```

## Components

### Card
```razor
<Card Style="margin-bottom: 2rem;">
    <p>Content goes here</p>
</Card>
```

### Button
```razor
<Button Appearance="accent" OnClick="HandleClick">
    Click Me
</Button>
```

**Appearance options:** `accent`, `primary`, `neutral`, `success`, `danger`

### Badge
```razor
<Badge Appearance="success">Active</Badge>
```

**Appearance options:** `accent`, `neutral`, `success`, `warning`, `error`

### TextField
```razor
<TextField Label="Name" @bind-Value="name" />
```

### NumberField
```razor
<NumberField Label="Count" @bind-Value="count" Min="1" Max="100" />
```

### TextArea
```razor
<TextArea Label="Description" @bind-Value="description" Rows="4" />
```

### ProgressBar
```razor
<ProgressBar Value="progress" Max="100" />
```

## Color System

### Background Colors
- `--color-bg-primary`: Main background (#0a0a0a)
- `--color-bg-secondary`: Elevated surfaces (#0f172a)
- `--color-bg-tertiary`: Cards and inputs (#1e293b)
- `--color-bg-elevated`: Hover states (#334155)

### Text Colors
- `--color-text-primary`: Primary text (#ffffff)
- `--color-text-secondary`: Secondary text (#c8c8c8)
- `--color-text-muted`: Muted text (#9ca3af)
- `--color-text-disabled`: Disabled state (#64748b)

### Accent Colors
- `--color-accent-primary`: Primary accent (#3b82f6)
- `--color-accent-primary-hover`: Hover state (#2563eb)
- `--color-success`: Success state (#10b981)
- `--color-warning`: Warning state (#f59e0b)
- `--color-error`: Error state (#ef4444)

## Spacing System

```css
--spacing-xs: 0.25rem;   /* 4px */
--spacing-sm: 0.5rem;    /* 8px */
--spacing-md: 1rem;      /* 16px */
--spacing-lg: 1.5rem;    /* 24px */
--spacing-xl: 2rem;      /* 32px */
--spacing-2xl: 3rem;     /* 48px */
```

## Typography

### Font Sizes
```css
--font-size-xs: 0.75rem;    /* 12px */
--font-size-sm: 0.875rem;   /* 14px */
--font-size-base: 1rem;     /* 16px */
--font-size-lg: 1.125rem;   /* 18px */
--font-size-xl: 1.25rem;    /* 20px */
--font-size-2xl: 1.5rem;    /* 24px */
--font-size-3xl: 2rem;      /* 32px */
```

### Font Weights
```css
--font-weight-normal: 400;
--font-weight-medium: 500;
--font-weight-semibold: 600;
--font-weight-bold: 700;
```

## Utility Classes

### Margin
- `.mb-1` through `.mb-5`: Margin bottom
- `.mt-1` through `.mt-5`: Margin top

### Padding
- `.p-1` through `.p-5`: Padding all sides

### Text
- `.text-center`: Center text
- `.text-muted`: Apply muted text color

## Creating a New Theme

1. **Add theme variant to `theme.css`:**
```css
[data-theme="cyberpunk"] {
    --color-bg-primary: #0d001a;
    --color-accent-primary: #ff00ff;
    /* ... customize all variables */
}
```

2. **Apply theme:**
```html
<body data-theme="cyberpunk">
```

3. **Or toggle programmatically:**
```csharp
@inject IJSRuntime JS

await JS.InvokeVoidAsync("eval", 
    "document.body.setAttribute('data-theme', 'cyberpunk')");
```

## Best Practices

1. **Always use CSS variables** instead of hardcoded colors
2. **Use semantic component names** (Card, Button) not visual names
3. **Keep spacing consistent** using the spacing scale
4. **Use utility classes** for common patterns
5. **Test theme changes** by switching `data-theme` attribute

## Migration from FluentUI

Replace FluentUI components with custom components:

| FluentUI | Custom Component |
|----------|-----------------|
| `<FluentCard>` | `<Card>` |
| `<FluentButton>` | `<Button>` |
| `<FluentBadge>` | `<Badge>` |
| `<FluentTextField>` | `<TextField>` |
| `<FluentNumberField>` | `<NumberField>` |
| `<FluentTextArea>` | `<TextArea>` |
| `<FluentProgress>` | `<ProgressBar>` |

## Troubleshooting

### White backgrounds appearing
Check that:
1. `theme.css` is imported in `app.css`
2. Components use CSS variables (`var(--color-bg-primary)`)
3. No inline styles override theme colors

### Styles not applying
1. Clear browser cache (Ctrl+F5)
2. Check browser DevTools for CSS variable values
3. Ensure components are in `Components/UI` folder
4. Verify `_Imports.razor` includes the UI namespace

## Future Enhancements

- [ ] Theme switcher component
- [ ] Additional theme presets (light, high contrast)
- [ ] Dark/light mode toggle
- [ ] User theme preferences persistence
- [ ] Animation system
- [ ] Responsive breakpoints
