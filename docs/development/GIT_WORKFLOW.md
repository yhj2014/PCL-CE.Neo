# Git 工作流

## 分支策略

我们使用 Git Flow 的变体：

```
main (主分支，稳定)
  └── develop (开发分支)
        ├── feature/phase1-arch (阶段功能分支)
        ├── feature/windows-platform (平台实现)
        └── release/v3.0 (发布分支)
```

## 分支说明

### main
- 始终稳定可发布
- 只接受来自 release 分支的合并
- 每个提交都应该是可部署的

### develop
- 集成开发分支
- 所有功能开发最终合并到这里
- 保持相对稳定

### feature/*
- 新功能开发分支
- 从 develop 分支创建
- 完成后合并回 develop
- 命名规范：`feature/<功能描述>`

### release/*
- 发布准备分支
- 从 develop 分支创建
- 只做 bug 修复和文档更新
- 完成后合并到 main 和 develop

## 工作流程

### 开发新功能

```bash
# 1. 更新本地 develop
git checkout develop
git pull origin develop

# 2. 创建功能分支
git checkout -b feature/my-feature

# 3. 开发并提交
git add .
git commit -m "feat: add my feature"

# 4. 推送到远程
git push origin feature/my-feature

# 5. 创建 Pull Request
# 等待代码审查和 CI 检查通过
# 合并到 develop
```

### 发布流程

```bash
# 1. 创建发布分支
git checkout develop
git checkout -b release/v3.0

# 2. 版本更新和最终测试
# 更新版本号
# 修复 bug

# 3. 合并到 main
git checkout main
git merge --no-ff release/v3.0
git tag -a v3.0 -m "Release version 3.0"

# 4. 合并回 develop
git checkout develop
git merge --no-ff release/v3.0

# 5. 推送
git push origin main --tags
git push origin develop
```

## 提交信息规范

请参考 [代码规范](CODING_STANDARDS.md) 中的 Git 提交规范部分。

## Pull Request 流程

1. 创建 PR，使用 PR 模板
2. 确保 CI 通过
3. 至少一名团队成员审查
4. 解决所有评论
5. Squash 合并（保持提交历史清洁）

## 代码审查清单

- [ ] 代码符合项目规范
- [ ] 有适当的单元测试
- [ ] 更新相关文档
- [ ] CI 检查通过
- [ ] 代码逻辑清晰且可维护
- [ ] 没有引入安全漏洞
