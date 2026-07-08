# Hero banners

Each log entry is assigned one of these banner keys at creation time (see
`LogEntryAggregate.HeroBanners` on the backend). Drop a matching image here named
`<key>.jpg` and it will render as the post's hero banner.

Expected files:

- `ruins.jpg`
- `forest.jpg`
- `dungeon.jpg`
- `tavern.jpg`
- `battlefield.jpg`
- `cave.jpg`

Recommended size ~1200×320 (roughly 15:4). Until a file exists, a themed gradient
fallback is shown instead (see `BANNER_GRADIENTS` in `TimelineView.tsx`), so the UI
never breaks on a missing image.
