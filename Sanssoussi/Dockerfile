FROM mcr.microsoft.com/dotnet/sdk:10.0

# Run as an unprivileged user instead of root. The id matches the host user (1000 by default)
# so that the files written in the mounted data folder belong to that user.
ARG UID=1000
RUN mkdir -p /app/data /home/web && chown -R ${UID} /app /home/web
USER ${UID}
ENV HOME=/home/web DOTNET_CLI_HOME=/home/web
WORKDIR /app

# Self-signed development certificate so the site can be served over HTTPS
RUN dotnet dev-certs https

# Restore packages first so this layer is cached between builds.
# --locked-mode fails the build if a package differs from packages.lock.json
COPY --chown=${UID} Sanssoussi.csproj packages.lock.json ./
RUN dotnet restore --locked-mode

COPY --chown=${UID} . .
RUN dotnet build --no-restore

# Production mode: generic error page, HSTS, no developer exception page
ENV ASPNETCORE_ENVIRONMENT=Production

# The database and the logs live in /app/data, which docker-compose maps to a folder on the host.
# The database is created by the migrations at startup; it is not shipped with the code.
ENV ConnectionStrings__SanssoussiContextConnection="Data Source=/app/data/Sanssoussi.db"
ENV LogDirectory=/app/data/logs

# Placeholder Google OpenID Connect credentials for the proof of concept.
# Real values are supplied at run time (see docker-compose.yml).
ENV Authentication__Google__ClientId=ID123
ENV Authentication__Google__ClientSecret=SECRET123

EXPOSE 5000 5001

# umask 077: only the application user may read or write the database and the logs.
CMD ["sh", "-c", "umask 077 && exec dotnet run --no-build --no-launch-profile --urls 'https://0.0.0.0:5001;http://0.0.0.0:5000'"]
