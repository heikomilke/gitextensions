FROM mono:6.12
RUN printf 'deb http://archive.debian.org/debian buster main\ndeb http://archive.debian.org/debian-security buster/updates main\n' > /etc/apt/sources.list \
 && echo 'Acquire::Check-Valid-Until "false";' > /etc/apt/apt.conf.d/99archive \
 && apt-get update && apt-get install -y --no-install-recommends git ca-certificates libgdiplus fonts-dejavu-core libcanberra-gtk-module && rm -rf /var/lib/apt/lists/*
