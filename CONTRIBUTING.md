# Contributing to Ignite

Thanks for contributing to **Ignite** — a next-generation server framework and web control panel for Space Engineers.

This project is focused on performance, extensibility, and long-term maintainability. Please follow the guidelines below to keep contributions consistent and high quality.

---

## 🚀 Getting Started

1. Fork the repository
2. Clone your fork:

   ```bash
   git clone https://github.com/<your-username>/ignite.git
   ```
3. Create a branch:

   ```bash
   git checkout -b feature/my-feature
   ```

---

## 🧱 Project Overview

Ignite is structured into multiple components:

* `IgniteSE1/` – Core server framework (runtime + orchestration)
* `Torch2API/` – Shared contracts and abstractions
* `Torch2WebUI/` – Web-based control panel
* *(Planned)* Plugin system for extensibility

Keep responsibilities separated. Avoid tightly coupling these layers.

---

## 🛠 Development Principles

### ✔ Keep It Modular

* Core logic should not depend on UI/web layers
* Prefer interfaces and abstractions over concrete coupling

### ✔ Favor Simplicity

* Avoid over-engineering
* Write code that’s easy to read and maintain

### ✔ Extensibility First

* Design features with plugins/modules in mind
* Avoid hardcoding behavior that should be configurable

---

## 🧾 Code Style

* Follow standard C# conventions
* Use meaningful names (no abbreviations unless obvious)
* Keep methods small and focused
* Avoid deep nesting

If formatting changes are needed, submit them separately from logic changes.

---

## 🧪 Testing

* Add tests for new functionality when practical
* Ensure existing tests pass before submitting
* Don’t introduce breaking changes without discussion

---

## 🔄 Pull Requests

Before submitting a PR:

* Rebase or merge latest `main`
* Keep changes focused and minimal
* Write a clear description:

  * What does this change do?
  * Why is it needed?

### PRs may be rejected if they:

* Mix unrelated changes
* Introduce unnecessary complexity
* Break architecture boundaries

---

## 🐛 Issues & Bugs

When reporting issues, include:

* Clear description
* Steps to reproduce
* Expected vs actual behavior
* Logs/screenshots if applicable

---

## 💡 Feature Requests

* Explain the problem you’re solving
* Avoid vague suggestions
* Be open to discussion before implementation

---

## 🧩 Plugin System (Upcoming)

Ignite is being designed for extensibility.
Plugin development guidelines will be added as the system stabilizes.

Until then:

* Avoid building features that should be plugins into the core
* Propose extension points instead

---

## ⚠️ Things to Avoid

* Large, unfocused pull requests
* Breaking API changes without discussion
* Adding dependencies without clear justification
* Mixing refactors with feature work

---

## 🤝 Conduct

Be respectful and constructive.
This is a long-term project — collaboration quality matters.

---

## 🧠 Final Notes

Ignite is still evolving. Some areas may change rapidly.

If you're unsure about an approach, open a discussion before implementing.

---

Thanks for contributing 🚀
