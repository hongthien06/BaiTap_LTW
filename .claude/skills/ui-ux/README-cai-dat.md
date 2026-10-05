# Nguồn của skill này

Skill `ui-ux` lấy từ **evondevKit** — https://github.com/evondev/evondevKit (MIT).
Trang giới thiệu: https://evondev-uiux.vercel.app/en/ui-ux

Cài bằng cách copy thẳng `skills/ui-ux/` từ repo vào `.claude/skills/ui-ux/`, **không** dùng
`/plugin marketplace add` như hướng dẫn gốc, vì lệnh `/plugin` không chạy được trong môi
trường Claude Code của máy này.

Cập nhật về bản mới:

```bash
git clone --depth 1 https://github.com/evondev/evondevKit /tmp/evondevKit
rm -rf .claude/skills/ui-ux
cp -r /tmp/evondevKit/skills/ui-ux .claude/skills/ui-ux
```

Giấy phép gốc giữ ở `LICENSE-evondevKit`.
