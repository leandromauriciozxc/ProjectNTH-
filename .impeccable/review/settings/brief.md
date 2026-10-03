# Settings extension

Mode: Operate. Extend the existing Unity main menu. The user approved Master, Music, Sound Effects and Dialogue volume, graphics quality, fullscreen/resolution, mouse sensitivity, and 30/60/120/Unlimited FPS. The separate question about the meaning of VFX was unanswered; this version controls existing quality profiles, without inventing individual effect switches.

Inherit the current Credits Panel's OldCupboard font, pale ink (0.88, 0.92, 0.93), charcoal panel, red Back action, CanvasScaler at 1920×1080 and match 0.5. Use three labelled tabs, aligned option rows, horizontal volume sliders, previous/next display choices, and a timed display confirmation. No identity change, new raster art, or approved comp. This is a local menu extension; the implementation is code-led.

First viewport: Settings title and Audio/Display/Controls tabs, with four labelled audio sliders and numeric values. The active tab has a pale underline; keyboard focus remains separately visible. Opening fades in over 0.18 seconds using unscaled time. Back or Escape restores the existing menu. Readability and click boundaries take priority over decoration.

Required capture matrix: Audio at 1920×1080; Audio, Display and Controls at 1061×591 and 1024×768; display confirmation at 1061×591. Captures are actual Unity UI renders in an isolated fixture with a dark background, not gameplay screenshots. No web detector ran because this is Unity UGUI.
