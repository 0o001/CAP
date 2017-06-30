# CAP
[![Travis branch](https://img.shields.io/travis/dotnetcore/CAP/master.svg?label=travis-ci)](https://travis-ci.org/dotnetcore/CAP)
[![AppVeyor](https://ci.appveyor.com/api/projects/status/4mpe0tbu7n126vyw?svg=true)](https://ci.appveyor.com/project/yuleyule66/cap)
[![GitHub license](https://img.shields.io/badge/license-MIT-blue.svg)](https://raw.githubusercontent.com/dotnetcore/CAP/master/LICENSE.txt)

CAP ��һ���ڷֲ�ʽϵͳ��SOA��MicroService����ʵ������һ���ԵĿ⣬����������������ʹ�á������ܵ��ص㡣

## Ԥ����OverView��

CAP ����һ�� ASP.NET Core ��Ŀ��ʹ�õĿ⣬��Ȼ���������� ASP.NET Core On .NET Framework �С�

����԰� CAP ������һ�� EventBus����Ϊ������ EventBus �����й��ܣ�����CAP�ṩ�˸��Ӽ򻯵ķ�ʽ������ EventBus �еķ����Ͷ��ġ�

CAP ������Ϣ�־û��Ĺ��ܣ�����ķ��������������崻�ʱ�����Ա�֤��Ϣ�Ŀɿ��ԡ�CAP�ṩ�˻���Microsoft DI �� Producer Service ���������Ժ����ҵ���������޷��ϣ�����֧��ǿһ���Ե�����

����CAP����ASP.NET Core ΢����ܹ��е�һ��ʾ��ͼ��

![](http://images2015.cnblogs.com/blog/250417/201706/250417-20170630143600289-1065294295.png)

> ͼ��ʵ�߲��ִ����û����룬���߲��ִ���CAP�ڲ�ʵ�֡�

## Getting Started

### NuGet  ��δ����

��������������������������Ŀ�а�װ CAP��

��������Ϣ����ʹ�õ��� Kafka �Ļ�������ԣ�

```
PM> Install-Package DotNetCore.CAP.Kafka
```

��������Ϣ����ʹ�õ��� RabbitMQ �Ļ�������ԣ�

```
PM> Install-Package DotNetCore.CAP.RabbitMQ
```

CAP Ĭ���ṩ�� Entity Framwork ��Ϊ���ݿ�洢��

```
PM> Install-Package DotNetCore.CAP.EntityFrameworkCore
```

### Configuration

��������CAP�� Startup.cs �ļ��У����£�

```cs
public void ConfigureServices(IServiceCollection services)
{
	......

    services.AddDbContext<AppDbContext>();

    services.AddCap()
            .AddEntityFrameworkStores<AppDbContext>()
            .AddKafka(x => x.Servers = "localhost:9453");
}

public void Configure(IApplicationBuilder app)
{
	.....

    app.UseCap();
}

```

### ����

�� Controller ��ע�� `ICapProducerService` Ȼ��ʹ�� `ICapProducerService` ������Ϣ����

```cs
public class PublishController : Controller
{
	private readonly ICapProducerService _producer;

	public PublishController(ICapProducerService producer)
	{
		_producer = producer;
	}


	[Route("~/checkAccount")]
	public async Task<IActionResult> PublishMessage()
	{
		//ָ�����͵���Ϣͷ������
		await _producer.SendAsync("xxx.services.account.check", new Person { Name = "Foo", Age = 11 });

		return Ok();
	}
}

```

### ����

**Action Method**

��Action����� Attribute �����������Ϣ��

�����ʹ�õ��� Kafak ��ʹ�� `[KafkaTopic()]`, ����� RabbitMQ ��ʹ�� `[RabbitMQTopic()]`

```cs
public class PublishController : Controller
{
	private readonly ICapProducerService _producer;

	public PublishController(ICapProducerService producer)
	{
		_producer = producer;
	}


	[NoAction]
	[KafkaTopic("xxx.services.account.check")]
	public async Task CheckReceivedMessage(Person person)
	{
		Console.WriteLine(person.Name);
		Console.WriteLine(person.Age);     
		return Task.CompletedTask;
	}
}

```

**Service Method**

�����Ķ��ķ���û��λ�� Controller �У����㶩�ĵ�����Ҫ�̳� `IConsumerService`��

```cs

namespace xxx.Service
{
	public interface ISubscriberService
	{
		public void CheckReceivedMessage(Person person);
	}


	public class SubscriberService: ISubscriberService, IConsumerService
	{
		[KafkaTopic("xxx.services.account.check")]
		public void CheckReceivedMessage(Person person)
		{
			
		}
	}
}

```

Ȼ���� Startup.cs �е� `ConfigureServices()` ��ע�����  `ISubscriberService` ��

```cs
public void ConfigureServices(IServiceCollection services)
{
	services.AddTransient<ISubscriberService,SubscriberService>();
}
```

## ����

���׵���򵥵ķ���֮һ�����ǲ������ۺ��������⣨issue������Ҳ����ͨ���ύ�� Pull Request �������������ס�

### License

MIT
