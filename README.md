# Bangumi UWP

一个使用 [Bangumi API](https://github.com/bangumi/api) 开发的 [Bangumi 番组计划](https://bgm.tv) 第三方 Win10 客户端。

[![Build Status](https://dev.azure.com/Teachoc/Bangumi%20UWP/_apis/build/status/Teachoc.Bangumi?branchName=master)](https://dev.azure.com/Teachoc/Bangumi%20UWP/_build/latest?definitionId=5&branchName=master)

## 功能

- 支持用户登录
- 支持查看用户进度
- 支持查看用户收藏（每类限制25个）
- 支持查看时间表
- 支持搜索
- 详情页支持修改收藏和条目状态、评分和吐槽（不支持删除收藏）
- 完整的本地缓存策略
- 使用 bangumi-data 显示放送站点

## 安装

- 系统版本：Windows 10 17763 及以上

    [<img src='https://assets.windowsphone.com/85864462-9c82-451e-9355-a3d5f874397a/English_get-it-from-MS_InvariantCulture_Default.png' alt='English badge' width=284 height=104/>](https://www.microsoft.com/store/apps/9PLKXLTWSVXR)

## 编译环境

- Visual Studio Community 2019

    说明：编译之前，请考虑至 [Bangumi 开发者平台](https://bgm.tv/dev/app) 创建应用，将申请到的 App ID 等填入 Bangumi\MainPage.xaml.cs 对应的位置中。
