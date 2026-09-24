#!/usr/bin/env python3
"""Render an original Expanded Summoning creature icon source with Blender.

Run headless:

    blender --background --factory-startup --python render_creature_icon.py \
        -- --creature pony --out sources/pony.png [--samples 128]

Everything here is procedural and project-owned: the creature is built from
metaballs and mesh primitives with procedural materials, lit with a warm key,
a cool rim and a low fill, in front of a radial atmospheric backdrop inside a
dark bronze ring, in the framing the roster icons established. No game pixels,
downloaded model, texture or generative-model output is an input. The output is
the 1254 by 1254 RGBA source that tools/New-ExpandedSummoningIcons.ps1 turns
into the 128 by 128 production icon.

Metaball note: a metaball element renders smaller than its nominal radius
(about three quarters of it at the default threshold), so the radii below are
authored for the rendered size, not the nominal one.
"""
import argparse
import math
import sys

import bpy  # noqa: E402
from mathutils import Vector, Euler  # noqa: E402


SIZE = 1254
UP = Vector((0.0, 0.0, 1.0))


# --- scene helpers ---------------------------------------------------------------
def clear_scene():
    bpy.ops.wm.read_factory_settings(use_empty=True)
    scene = bpy.context.scene
    scene.render.engine = "CYCLES"
    scene.cycles.device = "CPU"
    scene.render.resolution_x = SIZE
    scene.render.resolution_y = SIZE
    scene.render.resolution_percentage = 100
    scene.render.image_settings.file_format = "PNG"
    scene.render.image_settings.color_mode = "RGBA"
    scene.render.image_settings.color_depth = "8"
    scene.render.film_transparent = False
    scene.view_settings.view_transform = "AgX"
    scene.view_settings.look = "AgX - Medium High Contrast"
    scene.cycles.use_denoising = True
    scene.cycles.denoiser = "OPENIMAGEDENOISE"
    scene.cycles.max_bounces = 6
    world = bpy.data.worlds.new("IconWorld")
    scene.world = world
    world.use_nodes = True
    background = world.node_tree.nodes["Background"]
    background.inputs["Color"].default_value = (0.02, 0.02, 0.025, 1.0)
    background.inputs["Strength"].default_value = 0.5
    return scene


def material(name, color, roughness=0.55, metallic=0.0, emission=None,
             emission_strength=0.0, subsurface=0.0, noise=None):
    """A principled material. `noise` = (scale, strength, darker colour)
    adds a procedural mottle so flat primitives read as hide, fur or skin."""
    mat = bpy.data.materials.new(name)
    mat.use_nodes = True
    tree = mat.node_tree
    principled = tree.nodes["Principled BSDF"]
    principled.inputs["Base Color"].default_value = (*color, 1.0)
    principled.inputs["Roughness"].default_value = roughness
    principled.inputs["Metallic"].default_value = metallic
    if subsurface:
        principled.inputs["Subsurface Weight"].default_value = subsurface
        principled.inputs["Subsurface Radius"].default_value = (0.4, 0.2, 0.1)
    if emission is not None:
        principled.inputs["Emission Color"].default_value = (*emission, 1.0)
        principled.inputs["Emission Strength"].default_value = emission_strength
    if noise is not None:
        scale, strength, darker = noise
        tex = tree.nodes.new("ShaderNodeTexNoise")
        tex.inputs["Scale"].default_value = scale
        tex.inputs["Detail"].default_value = 7.0
        tex.inputs["Roughness"].default_value = 0.72
        ramp = tree.nodes.new("ShaderNodeValToRGB")
        ramp.color_ramp.elements[0].position = 0.38
        ramp.color_ramp.elements[0].color = (*darker, 1.0)
        ramp.color_ramp.elements[1].position = 0.7
        ramp.color_ramp.elements[1].color = (*color, 1.0)
        mix = tree.nodes.new("ShaderNodeMix")
        mix.data_type = "RGBA"
        mix.inputs["Factor"].default_value = strength
        mix.inputs[6].default_value = (*color, 1.0)
        tree.links.new(tex.outputs["Fac"], ramp.inputs["Fac"])
        tree.links.new(ramp.outputs["Color"], mix.inputs[7])
        tree.links.new(mix.outputs[2], principled.inputs["Base Color"])
    return mat


def assign(obj, mat):
    obj.data.materials.clear()
    obj.data.materials.append(mat)


def smooth(obj, levels=2):
    for polygon in obj.data.polygons:
        polygon.use_smooth = True
    if levels:
        modifier = obj.modifiers.new("Subdivision", "SUBSURF")
        modifier.levels = levels
        modifier.render_levels = levels


def sphere(name, location, scale, mat, rotation=(0, 0, 0), segments=48):
    bpy.ops.mesh.primitive_uv_sphere_add(segments=segments, ring_count=segments // 2,
                                         location=location)
    obj = bpy.context.active_object
    obj.name = name
    obj.scale = scale
    obj.rotation_euler = Euler([math.radians(v) for v in rotation], "XYZ")
    smooth(obj, 1)
    assign(obj, mat)
    return obj


def cylinder(name, location, radius, depth, mat, rotation=(0, 0, 0), vertices=48):
    bpy.ops.mesh.primitive_cylinder_add(vertices=vertices, radius=radius, depth=depth,
                                        location=location)
    obj = bpy.context.active_object
    obj.name = name
    obj.rotation_euler = Euler([math.radians(v) for v in rotation], "XYZ")
    smooth(obj, 1)
    assign(obj, mat)
    return obj


def cone(name, location, radius, depth, mat, rotation=(0, 0, 0), radius2=0.0):
    bpy.ops.mesh.primitive_cone_add(vertices=48, radius1=radius, radius2=radius2,
                                    depth=depth, location=location)
    obj = bpy.context.active_object
    obj.name = name
    obj.rotation_euler = Euler([math.radians(v) for v in rotation], "XYZ")
    smooth(obj, 0)
    assign(obj, mat)
    return obj


def cone_along(name, start, direction, length, radius, mat, radius2=0.0):
    """A cone whose base sits at `start` and whose tip points along `direction`."""
    direction = Vector(direction).normalized()
    centre = Vector(start) + direction * (length / 2.0)
    bpy.ops.mesh.primitive_cone_add(vertices=48, radius1=radius, radius2=radius2,
                                    depth=length, location=centre)
    obj = bpy.context.active_object
    obj.name = name
    obj.rotation_euler = direction.to_track_quat("Z", "Y").to_euler()
    smooth(obj, 0)
    assign(obj, mat)
    return obj


def box(name, location, scale, mat, rotation=(0, 0, 0), bevel=0.0):
    bpy.ops.mesh.primitive_cube_add(size=1.0, location=location)
    obj = bpy.context.active_object
    obj.name = name
    obj.scale = scale
    obj.rotation_euler = Euler([math.radians(v) for v in rotation], "XYZ")
    if bevel > 0:
        modifier = obj.modifiers.new("Bevel", "BEVEL")
        modifier.width = bevel
        modifier.segments = 4
    for polygon in obj.data.polygons:
        polygon.use_smooth = bevel > 0
    assign(obj, mat)
    return obj


class Blob:
    """One metaball object: organic masses that merge into each other."""

    def __init__(self, name, mat, resolution=0.05):
        data = bpy.data.metaballs.new(name)
        data.resolution = resolution
        data.render_resolution = resolution
        data.threshold = 0.6
        self.obj = bpy.data.objects.new(name, data)
        bpy.context.collection.objects.link(self.obj)
        data.materials.append(mat)
        self.data = data

    def ball(self, location, radius, size=None, axis=None, stiffness=2.0):
        """A ball, or an ellipsoid whose local X follows `axis` and whose
        `size` scales (along axis, across, up). Radii are rendered sizes."""
        element = self.data.elements.new()
        element.type = "ELLIPSOID" if size else "BALL"
        element.co = Vector(location)
        element.radius = radius / 0.75
        element.stiffness = stiffness
        if size:
            element.size_x, element.size_y, element.size_z = size
        if axis is not None:
            element.rotation = Vector(axis).normalized().to_track_quat("X", "Z")
        return element

    def chain(self, start, end, radius_start, radius_end, count=8, stiffness=2.2):
        start, end = Vector(start), Vector(end)
        for index in range(count):
            t = index / (count - 1)
            self.ball(start.lerp(end, t), radius_start + (radius_end - radius_start) * t,
                      stiffness=stiffness)


def backdrop(inner, outer, cam_location, cam_target, lens,
             ring_color=(0.30, 0.19, 0.08)):
    """Radial atmospheric background plane and the dark bronze ring, both
    centred on the camera's view line and facing the camera, the ring sized
    to sit just inside the frame edge as the roster icons' ring does."""
    origin = Vector(cam_location)
    view = (Vector(cam_target) - origin).normalized()
    facing = (-view).to_track_quat("Z", "Y").to_euler()
    plane_centre = origin + view * ((6.0 - origin.y) / view.y)
    bpy.ops.mesh.primitive_plane_add(size=40.0, location=plane_centre,
                                     rotation=facing)
    plane = bpy.context.active_object
    plane.name = "Backdrop"
    mat = bpy.data.materials.new("BackdropMaterial")
    mat.use_nodes = True
    tree = mat.node_tree
    tree.nodes.clear()
    output = tree.nodes.new("ShaderNodeOutputMaterial")
    emission = tree.nodes.new("ShaderNodeEmission")
    emission.inputs["Strength"].default_value = 1.0
    coords = tree.nodes.new("ShaderNodeTexCoord")
    mapping = tree.nodes.new("ShaderNodeMapping")
    mapping.inputs["Scale"].default_value = (0.22, 0.22, 0.22)
    gradient = tree.nodes.new("ShaderNodeTexGradient")
    gradient.gradient_type = "SPHERICAL"
    ramp = tree.nodes.new("ShaderNodeValToRGB")
    ramp.color_ramp.elements[0].position = 0.0
    ramp.color_ramp.elements[0].color = (*outer, 1.0)
    ramp.color_ramp.elements[1].position = 0.9
    ramp.color_ramp.elements[1].color = (*inner, 1.0)
    cloud = tree.nodes.new("ShaderNodeTexNoise")
    cloud.inputs["Scale"].default_value = 1.6
    cloud.inputs["Detail"].default_value = 8.0
    mix = tree.nodes.new("ShaderNodeMix")
    mix.data_type = "RGBA"
    mix.blend_type = "MULTIPLY"
    mix.inputs["Factor"].default_value = 0.35
    tree.links.new(coords.outputs["Object"], mapping.inputs["Vector"])
    tree.links.new(mapping.outputs["Vector"], gradient.inputs["Vector"])
    tree.links.new(gradient.outputs["Fac"], ramp.inputs["Fac"])
    tree.links.new(coords.outputs["Object"], cloud.inputs["Vector"])
    tree.links.new(ramp.outputs["Color"], mix.inputs[6])
    tree.links.new(cloud.outputs["Color"], mix.inputs[7])
    tree.links.new(mix.outputs[2], emission.inputs["Color"])
    tree.links.new(emission.outputs["Emission"], output.inputs["Surface"])
    assign(plane, mat)
    bronze = material("Bronze", ring_color, roughness=0.42, metallic=1.0)
    ring_distance = (3.2 - origin.y) / view.y
    ring_centre = origin + view * ring_distance
    ring_radius = 0.90 * ring_distance * 18.0 / lens
    bpy.ops.mesh.primitive_torus_add(major_radius=ring_radius,
                                     minor_radius=0.026 * ring_radius,
                                     major_segments=160, minor_segments=24,
                                     location=ring_centre, rotation=facing)
    ring = bpy.context.active_object
    ring.name = "Ring"
    smooth(ring, 0)
    assign(ring, bronze)


def lights(key=(1.0, 0.85, 0.6), rim=(0.55, 0.75, 1.0), key_energy=420.0,
           rim_energy=380.0, fill_energy=70.0):
    def lamp(name, kind, location, color, energy, size=2.0):
        data = bpy.data.lights.new(name, kind)
        data.energy = energy
        data.color = color
        if kind == "AREA":
            data.size = size
        obj = bpy.data.objects.new(name, data)
        bpy.context.collection.objects.link(obj)
        obj.location = location
        direction = Vector((0, 0, 1.0)) - obj.location
        obj.rotation_euler = direction.to_track_quat("-Z", "Y").to_euler()
        return obj
    lamp("Key", "AREA", (-3.2, -3.5, 4.2), key, key_energy, 2.5)
    lamp("Rim", "AREA", (3.0, 2.6, 3.4), rim, rim_energy, 1.5)
    lamp("Fill", "AREA", (2.8, -4.0, 0.4), (0.8, 0.85, 1.0), fill_energy, 4.0)


def camera(location=(0.0, -7.2, 1.15), target=(0.0, 0.0, 1.0), lens=60.0):
    data = bpy.data.cameras.new("IconCamera")
    data.lens = lens
    obj = bpy.data.objects.new("IconCamera", data)
    bpy.context.collection.objects.link(obj)
    obj.location = location
    direction = Vector(target) - Vector(location)
    obj.rotation_euler = direction.to_track_quat("-Z", "Y").to_euler()
    bpy.context.scene.camera = obj
    return obj


# --- creatures ---------------------------------------------------------------------
def equine(pony):
    """Head, neck and chest of a pony (stocky, shaggy) or a horse (long,
    refined), three-quarter from the front, the face turned to the camera's
    left."""
    if pony:
        coat = material("Coat", (0.42, 0.27, 0.13), 0.65,
                        noise=(9.0, 0.5, (0.28, 0.17, 0.08)))
        mane = material("Mane", (0.14, 0.095, 0.06), 0.85,
                        noise=(16.0, 0.55, (0.05, 0.035, 0.022)))
        muzzle_color = (0.24, 0.16, 0.10)
        neck_len, neck_r0, neck_r1, head_len = 1.35, 0.72, 0.5, 1.3
        poll = Vector((-0.45, -0.1, 1.35))
    else:
        coat = material("Coat", (0.33, 0.13, 0.05), 0.55,
                        noise=(9.0, 0.45, (0.20, 0.08, 0.035)))
        mane = material("Mane", (0.07, 0.05, 0.035), 0.8,
                        noise=(16.0, 0.5, (0.025, 0.02, 0.015)))
        muzzle_color = (0.14, 0.09, 0.06)
        neck_len, neck_r0, neck_r1, head_len = 1.8, 0.58, 0.4, 1.55
        poll = Vector((-0.5, -0.15, 1.75))
    muzzle_mat = material("Muzzle", muzzle_color, 0.6)
    axis = Vector((-0.62, -0.5, -0.62)).normalized()      # poll to muzzle
    side = axis.cross(UP).normalized()                     # to the creature's right
    muzzle = poll + axis * head_len
    chest = Vector((0.4, 0.4, -0.05))
    body = Blob("Body", coat, 0.045)
    body.ball((0.45, 0.6, -0.95), 1.3, (1.35, 1.0, 0.85), axis=(1, 0, 0))
    body.ball((-0.35, 0.35, -0.8), 0.9)
    body.chain(chest, poll, neck_r0, neck_r1, count=9)
    head = Blob("Head", coat, 0.04)
    head.ball(poll + axis * (0.3 * head_len), 0.62, (1.25, 0.82, 0.95), axis=axis)
    head.ball(poll + axis * (0.52 * head_len) - UP * 0.12, 0.52, (1.25, 0.8, 0.85),
              axis=axis)
    head.ball(muzzle - axis * 0.28, 0.42, (1.0, 0.85, 0.85), axis=axis)
    head.ball(poll + axis * 0.1, 0.5, (0.9, 0.9, 0.9))
    sphere("Muzzle", muzzle - axis * 0.08, (0.3, 0.3, 0.26), muzzle_mat)
    nostril = material("Nostril", (0.03, 0.02, 0.02), 0.5)
    for s in (-1, 1):
        sphere("Nostril%d" % s, muzzle - axis * 0.02 + side * (s * 0.16) - UP * 0.02,
               (0.06, 0.05, 0.07), nostril)
    eye = material("Eye", (0.02, 0.015, 0.01), 0.12)
    eye_center = poll + axis * (0.34 * head_len) + UP * 0.1
    sphere("EyeL", eye_center - side * 0.46, (0.11, 0.1, 0.1), eye)
    sphere("EyeR", eye_center + side * 0.46, (0.11, 0.1, 0.1), eye)
    ear_len = 0.5 if pony else 0.58
    for s in (-1, 1):
        base = poll + side * (s * 0.26) + UP * 0.05
        cone_along("Ear%d" % s, base, Vector((0.0, -0.15, 1.0)) + side * (s * 0.35),
                   ear_len, 0.13, coat)
    # mane along the crest of the neck, forelock between the ears
    neck_dir = (poll - chest).normalized()
    crest = (UP - neck_dir * UP.dot(neck_dir)).normalized()
    hair = Blob("Mane", mane, 0.045)
    count = 11
    for index in range(count):
        t = index / (count - 1)
        wave = 0.12 * math.sin(index * 1.9)
        centre = chest.lerp(poll, t) + crest * ((0.46 if pony else 0.36) + wave)
        hair.ball(centre, (0.40 if pony else 0.27) * (1.0 - 0.15 * t), stiffness=2.4)
        # a second, lower row hanging down the near side of the neck
        hair.ball(centre - crest * 0.25 - side * (0.28 if pony else 0.2),
                  (0.26 if pony else 0.18) * (1.0 - 0.15 * t), stiffness=2.6)
    hair.ball(poll + crest * 0.25 + axis * 0.15, 0.36 if pony else 0.26)
    if pony:
        hair.ball(poll + axis * 0.38 + UP * 0.18, 0.3)
        hair.ball(poll + axis * 0.55 + UP * 0.12 - side * 0.12, 0.24)
    else:
        blaze = material("Blaze", (0.9, 0.86, 0.8), 0.6)
        for index in range(6):
            t = index / 5.0
            sphere("Blaze%d" % index,
                   poll + axis * ((0.22 + 0.62 * t) * head_len) + UP * (0.28 - 0.1 * t),
                   (0.1, 0.1, 0.12), blaze)


def owlbear():
    fur = material("Fur", (0.23, 0.14, 0.075), 0.85, noise=(8.0, 0.6, (0.11, 0.065, 0.035)))
    disc = material("FacialDisc", (0.55, 0.42, 0.25), 0.9,
                    noise=(12.0, 0.45, (0.38, 0.27, 0.15)))
    beak = material("Beak", (0.07, 0.06, 0.05), 0.35)
    claw = material("Claw", (0.32, 0.30, 0.27), 0.3)
    eye_amber = material("EyeAmber", (1.0, 0.6, 0.1), 0.25,
                         emission=(1.0, 0.5, 0.08), emission_strength=1.2)
    pupil = material("Pupil", (0.015, 0.01, 0.01), 0.2)
    body = Blob("Body", fur, 0.05)
    body.ball((0.0, 0.45, -1.1), 1.75, (1.35, 1.0, 0.85), axis=(1, 0, 0))     # chest
    body.ball((0.0, 0.6, -0.15), 1.15, (1.55, 0.9, 0.7), axis=(1, 0, 0))      # hump
    body.ball((0.0, 0.05, 0.45), 0.78)                                        # neck
    for s in (-1, 1):
        body.ball((s * 1.5, 0.15, -0.3), 0.75)                               # shoulders
        body.chain((s * 1.55, -0.1, -0.5), (s * 1.8, -0.95, -1.05), 0.5, 0.42, 5)
        body.ball((s * 1.8, -1.0, -1.1), 0.5)                                # paw
    head = Blob("Head", fur, 0.04)
    head.ball((0.0, 0.05, 1.3), 1.05, (1.1, 1.0, 1.0), axis=(1, 0, 0))
    head.ball((0.0, -0.3, 1.2), 0.82, (1.15, 0.6, 0.95), axis=(1, 0, 0))
    for s in (-1, 1):
        cone_along("Tuft%d" % s, (s * 0.7, 0.15, 2.0), (s * 0.4, -0.05, 1.0),
                   0.85, 0.15, fur)
        sphere("Disc%d" % s, (s * 0.44, -0.86, 1.3), (0.52, 0.12, 0.55), disc)
        sphere("Eye%d" % s, (s * 0.4, -0.98, 1.34), (0.24, 0.14, 0.24), eye_amber)
        sphere("Pupil%d" % s, (s * 0.4, -1.11, 1.34), (0.1, 0.05, 0.1), pupil)
    cone_along("Beak", (0.0, -0.95, 1.2), (0.0, -0.55, -1.0), 0.62, 0.2, beak, 0.02)
    for s in (-1, 1):
        for index in range(3):
            offset = (index - 1) * 0.2
            cone_along("Claw%d%d" % (s, index),
                       (s * (1.8 + offset * 0.5), -1.25 - abs(offset) * 0.15, -1.2),
                       (s * 0.15, -0.6, -1.0), 0.5, 0.085, claw)


def cyclops():
    skin = material("Skin", (0.5, 0.33, 0.2), 0.7, subsurface=0.12,
                    noise=(7.0, 0.5, (0.33, 0.19, 0.11)))
    hide = material("Hide", (0.25, 0.155, 0.08), 0.85, noise=(10.0, 0.5, (0.14, 0.085, 0.045)))
    strap = material("Strap", (0.1, 0.07, 0.045), 0.7)
    steel = material("Steel", (0.6, 0.62, 0.64), 0.3, metallic=1.0)
    haft = material("Haft", (0.15, 0.095, 0.05), 0.65, noise=(20.0, 0.5, (0.07, 0.045, 0.025)))
    eye_white = material("EyeWhite", (0.88, 0.84, 0.76), 0.3)
    iris = material("Iris", (0.85, 0.42, 0.08), 0.25, emission=(0.9, 0.38, 0.05),
                    emission_strength=0.5)
    pupil = material("Pupil", (0.015, 0.01, 0.01), 0.2)
    tusk = material("Tusk", (0.82, 0.77, 0.65), 0.45)
    body = Blob("Body", skin, 0.05)
    body.ball((0.0, 0.4, -1.1), 1.9, (1.5, 1.0, 0.8), axis=(1, 0, 0))          # chest
    body.ball((0.0, 0.05, 0.15), 0.72, (1.1, 1.0, 0.7), axis=(1, 0, 0))        # neck
    for s in (-1, 1):
        body.ball((s * 1.6, 0.2, -0.45), 0.9)                                 # shoulders
    body.chain((1.85, -0.2, -0.7), (2.05, -0.65, -1.5), 0.5, 0.42, 5)           # right arm
    body.ball((2.05, -0.7, -0.6), 0.46)                                        # fist on the haft
    head = Blob("Head", skin, 0.04)
    head.ball((0.0, 0.0, 1.15), 1.05, (1.0, 1.05, 1.15), axis=(1, 0, 0))
    head.ball((0.0, -0.2, 0.62), 0.78, (1.05, 0.9, 0.75), axis=(1, 0, 0))     # jaw
    head.ball((0.0, -0.42, 1.48), 0.55, (1.6, 0.6, 0.42), axis=(1, 0, 0))     # brow
    for s in (-1, 1):
        cone_along("Ear%d" % s, (s * 0.85, 0.1, 1.3), (s * 1.0, 0.05, 0.3), 0.62, 0.18, skin)
    sphere("Eye", (0.0, -0.82, 1.2), (0.32, 0.22, 0.32), eye_white)
    sphere("Iris", (0.0, -1.0, 1.2), (0.17, 0.09, 0.17), iris)
    sphere("Pupil", (0.0, -1.08, 1.2), (0.075, 0.04, 0.075), pupil)
    sphere("Mouth", (0.0, -0.88, 0.58), (0.34, 0.07, 0.045),
           material("MouthDark", (0.03, 0.02, 0.015), 0.6))
    for s in (-1, 1):
        cone_along("Tusk%d" % s, (s * 0.26, -0.8, 0.5), (0.0, -0.15, 1.0), 0.32, 0.065, tusk)
    for s in (-1, 1):
        sphere("Pauldron%d" % s, (s * 1.6, 0.05, -0.15), (0.72, 0.6, 0.45), hide)
    box("Strap", (0.1, -1.25, -0.85), (0.3, 0.06, 2.6), strap, (0, 34, 0), bevel=0.02)
    cylinder("Haft", (1.85, -0.75, 0.35), 0.08, 3.5, haft, (0, -5, 0))
    # a single-bladed axe head flaring out from its socket to a wide edge
    blade = box("Blade", (2.2, -0.75, 1.9), (0.85, 0.1, 0.7), steel, (0, -5, 0),
                bevel=0.03)
    for vertex in blade.data.vertices:
        if vertex.co.x > 0:
            vertex.co.z *= 1.6
            vertex.co.y *= 0.35
    box("Socket", (1.8, -0.75, 1.9), (0.34, 0.22, 0.56), steel, (0, -5, 0), bevel=0.03)


CREATURES = {
    "pony": dict(build=lambda: equine(True),
                 inner=(0.5, 0.38, 0.16), outer=(0.05, 0.04, 0.025),
                 key=(1.0, 0.88, 0.62), rim=(0.85, 0.85, 0.95),
                 camera=((1.2, -6.8, 1.35), (-0.35, -0.1, 0.75), 60.0)),
    "horse": dict(build=lambda: equine(False),
                  inner=(0.45, 0.26, 0.11), outer=(0.035, 0.025, 0.035),
                  key=(1.0, 0.82, 0.55), rim=(0.85, 0.8, 0.9),
                  camera=((1.3, -7.4, 1.6), (-0.4, -0.1, 0.95), 60.0)),
    "owlbear": dict(build=owlbear,
                    inner=(0.14, 0.28, 0.12), outer=(0.015, 0.025, 0.015),
                    key=(1.0, 0.86, 0.6), rim=(0.5, 0.8, 1.0),
                    camera=((0.0, -8.4, 1.3), (0.0, 0.0, 0.5), 55.0)),
    "cyclops": dict(build=cyclops,
                    inner=(0.24, 0.28, 0.34), outer=(0.015, 0.015, 0.025),
                    key=(1.0, 0.8, 0.55), rim=(0.55, 0.7, 1.0),
                    camera=((0.4, -7.8, 0.3), (0.35, 0.0, 0.7), 55.0)),
}


def main():
    argv = sys.argv[sys.argv.index("--") + 1:] if "--" in sys.argv else []
    parser = argparse.ArgumentParser()
    parser.add_argument("--creature", required=True, choices=sorted(CREATURES))
    parser.add_argument("--out", required=True)
    parser.add_argument("--samples", type=int, default=128)
    args = parser.parse_args(argv)
    spec = CREATURES[args.creature]
    scene = clear_scene()
    scene.cycles.samples = args.samples
    location, target, lens = spec["camera"]
    backdrop(spec["inner"], spec["outer"], location, target, lens)
    lights(spec["key"], spec["rim"])
    camera(location, target, lens)
    spec["build"]()
    scene.render.filepath = args.out
    bpy.ops.render.render(write_still=True)
    print("rendered", args.creature, "->", args.out)


if __name__ == "__main__":
    main()
