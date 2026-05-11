# Dreamine.Communication.Serial

`Dreamine.Communication.Serial`는 Dreamine Communication 계열 패키지의 일부입니다.

이 패키지는 시리얼 통신 전송 구현체를 제공하며, RS232 관련 책임을 상위 애플리케이션 계층과 분리합니다.

[➡️ English Version](README.md)

## Description

RS232 and SerialPort transport package for Dreamine Communication.

## 주요 기능

- SerialPort 기반 Transport
- RS232 통신 경계
- MessageEnvelope 기반 송수신 흐름
- Core의 공통 JSON 직렬화 사용
- Core의 공통 프레임 처리 사용

## 설계 원칙

- 구체 통신 구현체를 상위 레이어와 분리합니다.
- `Dreamine.Communication.Abstractions`의 계약에 의존합니다.
- 패키지 책임을 작고 명확하게 유지합니다.
- 단방향 의존성 흐름을 유지합니다.
- 향후 어댑터를 추가해도 애플리케이션 로직을 변경하지 않도록 합니다.

## 패키지 역할

```text
Dreamine.Communication.Abstractions
    ↑
Dreamine.Communication.Core
    ↑
Dreamine.Communication.Serial
```

## 의존성

- `Dreamine.Communication.Abstractions`
- `Dreamine.Communication.Core`
- `System.IO.Ports`

## 대상 프레임워크

```text
net8.0
```

## 관련 패키지

- `Dreamine.Communication.Abstractions`
- `Dreamine.Communication.Core`
- `Dreamine.Communication.Sockets`
- `Dreamine.Communication.Serial`
- `Dreamine.Communication.RabbitMQ`
- `Dreamine.Communication.FullKit`
- `Dreamine.Communication.Wpf`

## 라이선스

이 프로젝트는 MIT 라이선스를 따릅니다.
