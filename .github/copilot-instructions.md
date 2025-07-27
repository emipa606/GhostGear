# GitHub Copilot Instructions for the RimWorld GhostGear Mod

## Mod Overview and Purpose

The GhostGear mod for RimWorld extends the gameplay mechanics by introducing innovative apparel and utility systems, aiming to enhance player strategies and combat experiences. This includes the integration of gear such as grappling hooks, caltrops, and specialized shields.

## Key Features and Systems

- **GhostGear Apparel:** Advanced apparel with special properties that can absorb damage and offer additional tactical options.
- **Grapple Hook Mechanic:** Gives pawns the ability to quickly navigate through the map using grappling hooks.
- **Caltrops Deployment:** Strategic placement of caltrops for area denial and damage dealing to enemy pawns.
- **PelShield:** A robust shielding system that can automatically repair under certain conditions, offering enhanced defense mechanisms.

## Coding Patterns and Conventions

- **Naming Conventions:** We use the PascalCase style for class names and camelCase for method names. Each class is prefixed with `GG` (GhostGear) or `HW` (Haywire) to signify its module.
- **Class Inheritance:** Most systems and mechanics extend from existing RimWorld classes. For example, `JobDriver_GGGrappleHook` extends `JobDriver_Wait` for time-based actions.
- **Encapsulation:** Methods are generally private unless they need to interact across classes or with RimWorld's systems.

## XML Integration

- **LoadDef XMLs:** Integration with XML files is crucial for defining new items, jobs, and behaviors. Ensure XML files define compatible ThingDefs and WorkGiverDefs for each new mechanic.

*Note: Several XML files are currently experiencing parsing issues. Please review and validate these XML files for correct syntax and structure.*

## Harmony Patching

Harmony patches are extensively used to modify the base game functionality without directly altering the original code. Key patches include:

- **CanWearTogether_GGPostPatch:** Modifies apparel compatibility logic to incorporate GhostGear items.
- **CheckPreAbsorbDamage_PostPatch:** Intercepts the damage absorption process to inject custom behavior.
- **Notify_DamageApplied_PostPatch:** Used to trigger specific actions or events when damage is applied to a character or item.

## Suggestions for Copilot

1. **Code Completion:** Use Copilot to generate boilerplate code for new job drivers or apparel items by providing descriptive comments on their intended functions.
2. **XML Helpers:** Leverage Copilot to craft XML snippet completions, ensuring all required tags and properties are present for new definitions.
3. **Patch Generation:** Copilot can assist in quickly drafting Harmony patches by suggesting method signatures and match components when base game methods need adjusting.
4. **Error Handling:** Implement comprehensive try-catch blocks in Copilot suggestions to manage potential runtime errors effectively, especially when interacting with game data.
5. **Method Documentation:** Encourage Copilot to autogenerate XML documentation comments for newly implemented methods to enhance readability and maintenance.

By adhering to these guidelines, you can effectively harness GitHub Copilot to streamline the development process for the GhostGear RimWorld mod, ensuring robust and feature-rich gameplay enhancements.
