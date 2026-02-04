plugins {
    id("java")
    id("org.jetbrains.kotlin.jvm") version "1.9.20"
    id("org.jetbrains.intellij") version "1.16.1"
}

group = "com.contesting"
version = "1.0.0"

repositories {
    mavenCentral()
}

dependencies {
    implementation("com.google.code.gson:gson:2.10.1")
}

intellij {
    version.set("2024.2")
    type.set("RD")  // Rider
    plugins.set(listOf("rider-plugins-appender"))
}

tasks {
    patchPluginXml {
        sinceBuild.set("242")
        untilBuild.set("999.*")  // Support all future versions
    }

    runIde {
        // Launches Rider with plugin for testing
    }

    withType<org.jetbrains.kotlin.gradle.tasks.KotlinCompile> {
        kotlinOptions.jvmTarget = "17"
    }
}

kotlin {
    jvmToolchain(17)
}
