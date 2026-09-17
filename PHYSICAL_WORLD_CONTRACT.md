# UNITYLAPTOP Physical World Contract

This repository is the gameplay source of truth. Runtime visuals, collision, interaction, and scale must describe the same physical object.

- Player reference: approximately 1.8 m tall with a 0.34 m CharacterController radius.
- Interior walls: approximately 3.2 m tall.
- Ordinary doors: approximately 2.18 m high by 1.1 m wide. The visible leaf is the blocker; it pivots from a physical edge hinge, remains collidable while moving, can close again, and must abort movement rather than sweep through the player.
- Containers: hollow shell geometry plus a real moving lid. The lid stays collidable while moving. Contents stay inactive and unreachable until the lid is fully open.
- Local interactions execute on the input frame. Reach/hand animation is presentation and must not delay gameplay state changes.
- Physical obstruction probes include the player even when the player is intentionally placed on Ignore Raycast.
- Cinematic encounter geometry follows the same rules and is not to be replaced by invisible blockers.

The compatibility bridge in `karlokalinic/UNITYLAPTOP` is deployment infrastructure only. Gameplay changes belong here on `main`; the bridge publishes an exact canonical snapshot to Unity Build Automation and production Cloudflare deployment.
