# FunnyProjector

A simple mod to set custom images as greenscreen backgrounds.

The basic idea behind its design is that there is no need for everyone to store images locally:
just URL's from which the mod will load images. At the same time not only the host can add theirs
images, but also those to whom they allowed (this is configurable in the mod's config).

The mod can be customized by editing ``FunnyProjector.Backgrounds.txt`` and ``FunnyProjector.cfg``
files manually or with r2modman or BepInEx.ConfigurationManager (it does not support the backgrounds list file).

**MyceliumNetworking is required**

## How to use
Add image URLs (PNG or JPEG) to ``BepInEx/config/FunnyProjector.Backgrounds.txt`` (one URL per line)

## Available settings
* ``Keep vanilla`` - keep vanilla projector background
* ``Allowed from`` - accepts ``Friends``, ``Host`` or ``Everyone``, determines who can suggest URLs