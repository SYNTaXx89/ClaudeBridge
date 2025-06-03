# ClaudeBridge

A modern .NET application built with .NET 9 Minimal APIs and .NET Aspire, providing a seamless integration with Claude AI.

## Purpose

ClaudeBridge serves as an API adapter that enables LLM agents designed for OpenAI's API specification to work with Claude AI. Many AI agents and applications are built around the OpenAI API structure, but organizations may want to use Claude AI instead. This project bridges that gap by:

- Providing a compatible API interface that matches OpenAI's specification
- Enabling seamless integration of Claude AI with existing agent architectures
- Allowing organizations to switch between different LLM providers without changing their agent code

This adapter makes it possible to use Claude AI with any agent or application that was originally designed for OpenAI's API, simplifying the transition and integration process.

## Technologies

- **.NET 9**: Utilizing the latest features of .NET 9 with Minimal APIs for efficient and clean code
- **.NET Aspire**: Microsoft's cloud-native stack for building distributed applications
- **Swagger/OpenAPI**: API documentation available at `{applicationurl}/swagger`
- **Azure**: Cloud deployment platform

## Features

- Modern .NET 9 Minimal API architecture
- Cloud-native design with .NET Aspire
- Interactive API documentation with Swagger
- Azure deployment support

## Getting Started

### Prerequisites

- .NET 9 SDK
- Azure CLI
- Azure Developer CLI (azd)

### Local Development

1. Clone the repository
2. Run the application:
   ```bash
   dotnet run
   ```
3. Access the Swagger documentation at `http://localhost:{port}/swagger`

### Azure Deployment

The application can be easily deployed to Azure using the Azure Developer CLI:

1. Initialize the Azure Developer environment:
   ```bash
   azd init
   ```

2. Deploy to Azure:
   ```bash
   azd up
   ```

This will provision all necessary Azure resources and deploy your application.

## API Documentation

The API documentation is available through Swagger UI at `{applicationurl}/swagger`. This provides:
- Interactive API testing
- Detailed endpoint documentation
- Request/response schemas
- Authentication requirements

## Contributing

1. Fork the repository
2. Create your feature branch
3. Commit your changes
4. Push to the branch
5. Create a new Pull Request

## License

This project is licensed under the MIT License - see the LICENSE file for details. 