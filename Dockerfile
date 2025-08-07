# Build stage
ARG SHAREDSERVICES_ECR_REGISTRY
FROM ${SHAREDSERVICES_ECR_REGISTRY}/amazonlinux2023-dotnet9 AS build-env

# Set working directory
WORKDIR /app

# Copy published files
COPY ./publish .
COPY renew-kbr-ticket.sh /app/renew-kbr-ticket.sh

RUN chmod 644 /etc/krb5.conf
RUN chmod +x /app/renew-kbr-ticket.sh


COPY certs/tarwebs.crt /etc/pki/ca-trust/source/anchors/tarwebs.crt
RUN update-ca-trust


# Set environment variables
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false
ENV ASPNETCORE_URLS=http://+:80

# Expose port 80
EXPOSE 80

# Set the entry point
ENTRYPOINT ["dotnet", "bend-fund-wizard-poc.dll"]
