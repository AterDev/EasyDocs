# 作者的官方博客

作者的官方博客地址： [https://dusi.dev](https://www.dusi.dev)

![show](../images/show.png)

## Markdown样式内容

二级标题

### 标题

三级标题

---

## 代码块内容

HTML代码块

```html
<html>
    <head>
        <title>标题</title>
    </head>
    <body>
        <h1>标题</h1>
    </body>
</html>
```

CSS代码块

```css
body {
    background-color: #f0f0f0;
}
```

JavaScript代码块

```javascript
console.log('Hello World!');
```

Csharp代码块

```csharp
using System;

namespace MyBlog
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello from C#!");
        }
    }
}
```

## 列表内容

无序列表

- 无序列表1
- 无序列表2
- 无序列表3

有序列表

1. 有序列表1
2. 有序列表2
3. 有序列表3

## 表格内容

| 项目               | 价格      | 数量 | 说明                                 | 备注               |
| ------------------ | --------- | ---- | ------------------------------------ | ------------------ |
| iPhone max pro 111 | $560.0000 | 100000    | 说明内容有可以很长，超出了有限的宽度 | 仅备注说明测试内容 |
| iPad               | $780      | 2    | 留白                                 | 备注说明           |
| iMac               | $1999     | 1    | 留白                                 | 备注说明           |


### 图表

```mermaid
flowchart TB
    subgraph 外部网络
        U[用户 / 浏览器]
        T[机台 EDA Server<br/>SEMI Interface A Freeze 2]
    end

    subgraph 数据中心服务器集群
        subgraph 应用服务器组
            K3S[K3s 集群<br/>微服务容器运行环境]
        end

        subgraph 基础服务服务器组
            subgraph 数据库
                PG[(PostgreSQL<br/>业务数据库<br/>OLTP)]
                CH[(ClickHouse<br/>采集/分析数据库<br/>OLAP)]
            end
            subgraph 缓存
                REDIS[(Redis<br/>缓存 / 会话)]
            end
            subgraph 消息队列
                MQ[(Nats / RabbitMQ<br/>消息队列)]
            end
        end

        subgraph 自动化与运维
            GITLAB[GitLab<br/>代码仓库]
            RUNNER[GitLab Runner<br/>CI 构建]
            ARGOCD[ArgoCD<br/>CD 部署]
            REGISTRY[镜像仓库<br/>Nexus Repository]
            MONITOR[监控告警<br/>Prometheus + Grafana]
            LOG[(日志收集<br/>Loki / ELK)]
        end
    end

    U -->|HTTPS| K3S
    T -->|Interface A / HTTPS| K3S

    K3S --> PG
    K3S --> CH
    K3S --> REDIS
    K3S --> MQ
    K3S --> MONITOR
    K3S --> LOG

    GITLAB --> RUNNER
    RUNNER --> REGISTRY
    REGISTRY --> ARGOCD
    ARGOCD --> K3S
```


## 引用内容

> 这是普通的引用

> [!NOTE]
> Information the user should notice even if skimming

> [!IMPORTANT]
> Essential information required for user success

> [!WARNING]
> Dangerous certain consequences of an action
