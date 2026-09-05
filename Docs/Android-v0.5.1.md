# v0.5.1 安卓补充

在已归档的 v0.5.1 基础上补一份安卓安装包，游戏规则、画面和音效不另做调整。合成使用 Merge_B，移动起步继续用原来的拔剑音效。

构建入口仍是 Kait / Build Android Demo。修正了脚本里遗留的 0.4 文件名和版本号，输出为 Build/kait-v0.5.1.apk，版本号 0.5.1，版本代码 501。

目标为 Android 8.0 及以上的 ARM64 设备，使用 IL2CPP，保留 UnityPlayerActivity 启动入口修正，避免重新引入之前的启动类缺失问题。手机触控和滑动输入沿用现有实现。

已有源码和 Windows 包见 GitHub 的 v0.5.1 归档。本次没有覆盖旧 APK。构建开始前 ADB 未发现设备，打包成功不等于已完成真机测试。

最终安装包由提交 779569a 的独立工作目录构建，Unity 返回码 0。已检查包内 Merge_B 引用存在、未归档的 KaitIceBinding 类型不存在，启动入口为 UnityPlayerActivity。第一次在活动项目里生成的混合包不交付，移入本地 Backups/android-mixed-build-20260905。

正式 APK 大小 131000630 字节，SHA256：`5D30699964C35526717973BFCACC3051670C72647949A4C098DDCE2B966ACCC9`。
