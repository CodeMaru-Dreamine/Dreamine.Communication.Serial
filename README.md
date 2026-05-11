# Dreamine.Communication.Serial

`Dreamine.Communication.Serial` is part of the Dreamine Communication package family.

This package provides serial communication transport implementations while keeping RS232-specific logic isolated from the upper application layer.

[➡️ 한국어 문서 보기](./README_KO.md)

## Description

RS232 and SerialPort transport package for Dreamine Communication.

## Features

- SerialPort based transport
- RS232 communication boundary
- MessageEnvelope based send and receive flow
- Shared JSON serialization from Core
- Shared framing from Core

## Design Principles

- Keep concrete transport implementations isolated from upper layers.
- Depend on `Dreamine.Communication.Abstractions` contracts.
- Keep package responsibilities small and explicit.
- Preserve one-way dependency flow.
- Allow future adapters to be added without changing application logic.

## Package Role

```text
Dreamine.Communication.Abstractions
    ↑
Dreamine.Communication.Core
    ↑
Dreamine.Communication.Serial
```

## Dependencies

- `Dreamine.Communication.Abstractions`
- `Dreamine.Communication.Core`
- `System.IO.Ports`

## Target Framework

```text
net8.0
```

## Related Packages

- `Dreamine.Communication.Abstractions`
- `Dreamine.Communication.Core`
- `Dreamine.Communication.Sockets`
- `Dreamine.Communication.Serial`
- `Dreamine.Communication.RabbitMQ`
- `Dreamine.Communication.FullKit`
- `Dreamine.Communication.Wpf`

## License

This project is licensed under the MIT License.
