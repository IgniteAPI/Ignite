# Contributing to Ignite

Thanks for your interest in contributing to **Ignite** — the next-generation server framework and web control panel for Space Engineers.

This document outlines how to contribute effectively and keep the project clean, maintainable, and scalable.

---

## 🚀 Getting Started

1. Fork the repository
2. Clone your fork:

   ```bash
   git clone https://github.com/your-username/Ignite.git
   ```
3. Create a branch:

   ```bash
   git checkout -b feature/my-feature
   ```

---

## 🧱 Project Structure (High-Level)

* `IgniteSE1/` – SE1 Core server console
* `Torch2API/` – Shared API and abstractions
* `Torch2WebUI/` – Web-based control panel

---

## 🛠 Development Guidelines

### Code Style

* Use consistent formatting (IDE default or `.editorconfig`)
* Prefer clarity over cleverness
* Keep methods small and focused
* Avoid unnecessary abstractions

### Naming

* Use clear, descriptive names
* Avoid abbreviations unless widely understood
* Follow standard C# naming conventions

### Architecture

* Favor composition over inheritance
* Keep core logic separate from UI/web concerns
* Design with extensibility in mind (plugins/modules)

---

## 🧪 Testing

* Add unit tests for new logic where applicable
* Ensure all existing tests pass before submitting
* Avoid breaking public APIs without discussion

---

## 🔄 Pull Request Process

1. Ensure your branch is up to date with `main`
2. Keep PRs focused and minimal
3. Provide a clear description:

   * What does this change do?
   * Why is it needed?
4. Link related issues if applicable

---

## 🐛 Reporting Issues

When opening an issue, include:

* Description of the problem
* Steps to reproduce
* Expected vs actual behavior
* Logs or screenshots if relevant

---

## 💡 Feature Requests

* Explain the use case clearly
* Avoid vague “add X” requests
* Be open to discussion and iteration

---

## ⚠️ What to Avoid

* Large, unfocused pull requests
* Breaking changes without discussion
* Mixing formatting changes with logic changes
* Adding dependencies without justification

---

## 🧩 Plugins & Extensions (Future)

Ignite is being built with extensibility in mind. Plugin guidelines will be added as the system stabilizes.

---

## 🤝 Code of Conduct

Be respectful, constructive, and collaborative. We’re building something long-term — keep it professional.

---

## 🧠 Final Notes

This project is evolving. Guidelines may change as the architecture matures.

If you're unsure about something, open a discussion before implementing.

---

Thanks for contributing to Ignite 🚀
